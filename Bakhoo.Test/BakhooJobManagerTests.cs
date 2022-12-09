using Bakhoo.Entity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Serilog;
using Xunit.Abstractions;

namespace Bakhoo.Test;

public class BakhooJobManagerTests
{
    private readonly ITestOutputHelper _output;

    public BakhooJobManagerTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private IServiceCollection ConfigureBakhooWithMocks(
        IServiceCollection services,
        Func<IBakhooWorker, BakhooJob, Task> workerRun)
    {
        services.AddTestLogging(_output);
        services.AddSingleton<BakhooLord>();
        services.AddMockedScoped<IBakhooJobRepository>((provider, mock) =>
        {
            var jobs = provider.GetRequiredService<List<BakhooJob>>();
            mock.Setup(x => x.GetJobsInBacklogAsync())
                .Returns(() => jobs
                    .Where(x => x.Submitted.HasValue && !x.Start.HasValue)
                    .ToAsyncEnumerable());
            mock.Setup(x => x.GetJobsToCancelAsync())
                .Returns(jobs
                    .Where(x => x.IsCancelling)
                    .ToAsyncEnumerable());
        });
        services.AddMockedScoped<IBakhooWorker>((provider, mock) =>
        {
            BakhooJob? job = null;
            Task? runTask = null;
            Task? cancelTask = null;
            mock.Setup(x => x.StartWorkerAsync(It.IsAny<Guid>(), ItIs.CT()))
                .Callback<Guid, CancellationToken>((jobId, ct) =>
                {
                    job = getJobBy(provider, jobId);
                    job.Start = DateTimeOffset.UtcNow;
                    runTask = workerRun(mock.Object, job);
                });
            mock.SetupGet(x => x.JobId).Returns(() =>
            {
                Assert.NotNull(job);
                return job.Id;
            });
            mock.Setup(x => x.Cancel()).Callback(() =>
            {
                Assert.NotNull(job);
                job.IsCancelling = true;
                job.CancelRequested = DateTimeOffset.UtcNow;
                cancelTask = Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    job.IsCancelled = true;
                    job.IsCancelling = false;
                    job.End = DateTimeOffset.UtcNow;
                });
            });
            mock.SetupGet(x => x.RunTask).Returns(() => runTask);
            mock.SetupGet(x => x.CancelationTask).Returns(() => cancelTask);
        });
        services.Configure<BakhooOptions>(options =>
        {
            options.MaxBacklogJobs = 3;
            options.MaxParallelJobs = 1;
            options.MaxHoursToDisplayCompletedJobs = 24;
        });

        return services;

        BakhooJob getJobBy(IServiceProvider provider, Guid jobId)
            => provider.GetRequiredService<List<BakhooJob>>()
                .Single(x => x.Id == jobId);
    }

    private async Task<ServiceProvider> StartJobManager(
        CancellationTokenSource cts,
        List<BakhooJob> fakeJobs,
        Func<IBakhooWorker, BakhooJob, CancellationTokenSource, Task> workerRun,
        bool isDebugging = false)
    {
        ServiceCollection services = new();
        services.AddSingleton<List<BakhooJob>>(fakeJobs);
        ConfigureBakhooWithMocks(
            services,
            workerRun: (worker, job) => workerRun(worker, job, cts));
        var provider = services.BuildServiceProvider();

        var jobManager = provider.GetRequiredService<BakhooLord>();

        await jobManager.StartAsync(cts.Token);

        return provider;
    }

    private async Task AssertJobs(
        ServiceProvider provider,
        Action keepAssertingUntilCancelled,
        bool isDebugging = false)
    {
        var assertCts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        await KeepAsserting.UntilCancelled(assertCts.Token,
            keepAssertingUntilCancelled, isDebugging);
    }

    private async Task StopJobManager(
        ServiceProvider provider
        )
    {
        var jobManager = provider.GetRequiredService<BakhooLord>();
        var stopCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await jobManager.StopAsync(stopCts.Token);
    }

    private async Task<ServiceProvider> TestJobManager(
        CancellationTokenSource cts,
        List<BakhooJob> fakeJobs,
        Func<IBakhooWorker, BakhooJob, CancellationTokenSource, Task> workerRun,
        Action keepAssertingUntilCancelled,
        bool isDebugging = false)
    {
        var provider = await StartJobManager(cts, fakeJobs, workerRun, isDebugging);
        await AssertJobs(provider, keepAssertingUntilCancelled, isDebugging);
        await StopJobManager(provider);
        return provider;
    }

    [Fact]
    public async Task TestSuccessfulJob()
    {
        var isDebugging = false;

        List<BakhooJob> fakeJobs = new()
        {
            new BakhooJob { Id = Guid.NewGuid(), Submitted = DateTimeOffset.UtcNow },
        };

        var cts = new CancellationTokenSource(
            TimeSpan.FromSeconds(isDebugging ? 1 * 24 * 60 * 60 : 5));

        using var provider = await TestJobManager(
            cts,
            fakeJobs,
            workerRun: async (worker, job, cts) =>
            {
                job.End = DateTimeOffset.UtcNow;
            },
            keepAssertingUntilCancelled: () =>
            {
                Assert.Collection(fakeJobs, new Action<BakhooJob>[]
                {
                    job =>
                    {
                        Assert.NotNull(job.Start);
                        Assert.NotNull(job.End);
                        Assert.False(job.HasError);
                        Assert.False(job.IsCancelling);
                        Assert.False(job.IsCancelled);
                    },
                });
            });
    }

    [Fact]
    public async Task TestJobCancellationBeforeJobStart()
    {
        var isDebugging = false;

        List<BakhooJob> fakeJobs = new()
        {
            new BakhooJob { Id = Guid.NewGuid(), Submitted = DateTimeOffset.UtcNow, IsCancelling = true },
        };

        var cts = new CancellationTokenSource(
            TimeSpan.FromSeconds(isDebugging ? 1 * 24 * 60 * 60 : 5));

        using var provider = await TestJobManager(
            cts,
            fakeJobs,
            workerRun: async (worker, job, cts) =>
            {
                //job.End = DateTimeOffset.UtcNow;
            },
            keepAssertingUntilCancelled: () =>
            {
                Assert.Collection(fakeJobs, new Action<BakhooJob>[]
                {
                    job =>
                    {
                        Assert.NotNull(job.Start);
                        Assert.NotNull(job.End);
                        Assert.False(job.HasError);
                        Assert.False(job.IsCancelling);
                        Assert.True(job.IsCancelled);
                    },
                });
            });
    }

    [Fact]
    public async Task TestJobCancellationAfterJobStart()
    {
        var isDebugging = false;

        List<BakhooJob> fakeJobs = new()
        {
            new BakhooJob { Id = Guid.NewGuid(), Submitted = DateTimeOffset.UtcNow },
        };

        var cts = new CancellationTokenSource(
            TimeSpan.FromSeconds(isDebugging ? 1 * 24 * 60 * 60 : 5));

        using var provider = await TestJobManager(
            cts,
            fakeJobs,
            workerRun: async (worker, job, cts) =>
            {
                worker.Cancel(); // call faked cancellation that updates the job state
            },
            keepAssertingUntilCancelled: () =>
            {
                Assert.Collection(fakeJobs, new Action<BakhooJob>[]
                {
                    job =>
                    {
                        Assert.NotNull(job.Start);
                        Assert.NotNull(job.End);
                        Assert.False(job.HasError);
                        Assert.False(job.IsCancelling);
                        Assert.True(job.IsCancelled);
                    },
                });
            });
    }

    [Fact]
    public async Task TestJobManagerCancellation()
    {
        var isDebugging = false;

        List<BakhooJob> fakeJobs = new()
        {
            new BakhooJob { Id = Guid.NewGuid(), Submitted = DateTimeOffset.UtcNow },
        };

        var cts = new CancellationTokenSource(
            TimeSpan.FromSeconds(isDebugging ? 1 * 24 * 60 * 60 : 5));

        // start Job Manager
        // wait until job start
        // stop Job Manager
        using var provider = await TestJobManager(
            cts,
            fakeJobs,
            workerRun: async (worker, job, cts) =>
            {
                while (!cts.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            },
            keepAssertingUntilCancelled: () =>
            {
                Assert.Collection(fakeJobs, new Action<BakhooJob>[]
                {
                    job => Assert.NotNull(job.Start),
                });
            });

        await AssertJobs(
            provider,
            keepAssertingUntilCancelled: () =>
            {
                Assert.Collection(fakeJobs, new Action<BakhooJob>[]
                {
                    job =>
                    {
                        Assert.NotNull(job.Start);
                        Assert.NotNull(job.End);
                        Assert.False(job.HasError);
                        Assert.False(job.IsCancelling);
                        Assert.True(job.IsCancelled);
                    },
                });
            },
            isDebugging);
    }
}
