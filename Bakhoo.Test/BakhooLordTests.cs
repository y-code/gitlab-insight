using Bakhoo.Entity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Serilog;
using Xunit.Abstractions;

namespace Bakhoo.Test;

[Collection("BakhooTest")]
public class BakhooLordTests
{
    private readonly ITestOutputHelper _output;

    public BakhooLordTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private IServiceCollection ConfigureBakhooWithMocks(
        IServiceCollection services,
        Func<IBakhooVassal, BakhooJob, CancellationToken, Task> vassalRun)
    {
        services.AddTestLogging(_output);
        services.AddSingleton<BakhooLord>();
        services.AddMockedScoped<IBakhooJobMonitor>((provider, mock) =>
        {
            mock.Setup(x => x.NotifyIssueImportJobUpdatedAsync(It.IsAny<Guid>(), ItIs.CT()))
                .Returns(Task.CompletedTask);
        });
        services.AddMockedScoped<IBakhooJobSequencer>((provider, mock) =>
        {
            var jobs = provider.GetRequiredService<List<BakhooJob>>();
            mock.Setup(x => x.GetJobsInBacklogAsync())
                .Returns(() => jobs
                    .Where(x => x.Submitted.HasValue && !x.Start.HasValue)
                    .Select(x => x.Id)
                    .ToAsyncEnumerable());
        });
        services.AddMockedScoped<IBakhooJobRepository>((provider, mock) =>
        {
            var jobs = provider.GetRequiredService<List<BakhooJob>>();
            mock.Setup(x => x.GetJobsToCancelAsync())
                .Returns(() => jobs
                    .Where(x => x.IsCancelling)
                    .ToAsyncEnumerable());
            mock.Setup(x => x.GetJobAsync(It.IsAny<Guid>(), ItIs.CT()))
                .Returns<Guid, CancellationToken>((jobId, ct) => Task.FromResult(
                    jobs.Single(x => x.Id == jobId)));
            mock.Setup(x => x.UpdateCancelledJobAsync(It.IsAny<Guid>(), It.IsAny<string>(), ItIs.CT()))
                .Returns<Guid, string, CancellationToken>((jobId, message, ct) =>
                {
                    var job = jobs.Single(x => x.Id == jobId);
                    job.IsCancelled = true;
                    job.IsCancelling = false;
                    job.Start ??= DateTimeOffset.UtcNow;
                    job.End = DateTimeOffset.UtcNow;
                    return Task.CompletedTask;
                });
        });
        services.AddMockedScoped<IBakhooVassal>((provider, mock) =>
        {
            BakhooJob? job = null;
            Task? runTask = null;
            Task? cancelTask = null;
            mock.Setup(x => x.StartAsync(It.IsAny<Guid>(), ItIs.CT()))
                .Callback<Guid, CancellationToken>((jobId, ct) =>
                {
                    job = getJobBy(provider, jobId);
                    job.Start = DateTimeOffset.UtcNow;
                    if (job.IsCancelled) return;
                    runTask = Task.Run(async () =>
                    {
                        await vassalRun(mock.Object, job, ct);
                    });
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

    private async Task<ServiceProvider> StartLord(
        List<BakhooJob> fakeJobs,
        Func<IBakhooVassal, BakhooJob, CancellationToken, Task> vassalRun,
        bool isDebugging = false)
    {
        ServiceCollection services = new();
        services.AddSingleton<List<BakhooJob>>(fakeJobs);
        ConfigureBakhooWithMocks(
            services,
            vassalRun: (vassal, job, ct) => vassalRun(vassal, job, ct));
        var provider = services.BuildServiceProvider();

        var lord = provider.GetRequiredService<BakhooLord>();

        await lord.StartAsync(new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token);

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

    private async Task<ServiceProvider> TestLord(
        List<BakhooJob> fakeJobs,
        Func<IBakhooVassal, BakhooJob, CancellationToken, Task> vassalRun,
        Action keepAssertingUntilCancelled,
        bool isDebugging = false)
    {
        var provider = await StartLord(fakeJobs, vassalRun, isDebugging);
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

        using var provider = await TestLord(
            fakeJobs,
            vassalRun: async (worker, job, ct) =>
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
            },
            isDebugging);
    }

    [Fact]
    public async Task TestJobCancellationBeforeJobStart()
    {
        var isDebugging = false;

        List<BakhooJob> fakeJobs = new()
        {
            new BakhooJob { Id = Guid.NewGuid(), Submitted = DateTimeOffset.UtcNow, IsCancelling = true },
        };

        using var provider = await TestLord(
            fakeJobs,
            vassalRun: async (worker, job, ct) =>
            {
                //job.End = DateTimeOffset.UtcNow;
                Assert.Fail("worker should not be started because the job has been being cancelled.");
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
            },
            isDebugging);
    }

    [Fact]
    public async Task TestJobCancellationAfterJobStart()
    {
        var isDebugging = false;

        List<BakhooJob> fakeJobs = new()
        {
            new BakhooJob { Id = Guid.NewGuid(), Submitted = DateTimeOffset.UtcNow },
        };

        using var provider = await TestLord(
            fakeJobs,
            vassalRun: async (vassal, job, ct) =>
            {
                vassal.Cancel(); // call faked cancellation that updates the job state
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

        // start Job Manager
        // wait until job start
        // stop Job Manager
        using var provider = await TestLord(
            fakeJobs,
            vassalRun: async (worker, job, ct) =>
            {
                // run until cancelled
                // and finishes without throwing exception
                while (!ct.IsCancellationRequested)
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
