using System;
using System.Text.Json;
using System.Xml.Linq;
using Bakhoo.Entity;
using Bakhoo.Entity.Test;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace Bakhoo.Test;

public class SampleJobA { }
public class SampleJobB { }

enum JobHandlerStep
{
    Starting,
    SuccessfullyCompleted,
}

record JobHandlerLog(Type JobHandlerType, object Job, JobHandlerStep Step);

class JobHandlerTestLogger
{
    private readonly List<JobHandlerLog> _logs = new();
    public IEnumerable<JobHandlerLog> Logs
        => _logs.AsEnumerable();
    public void Log<TJobHandler>(object job, JobHandlerStep step)
        => _logs.Add(new(typeof(TJobHandler), job, step));
}

abstract class SampleJobHandler<TJob, TImpl> : IBakhooJobHandler<TJob>
    where TImpl : IBakhooJobHandler<TJob>
{
    private readonly JobHandlerTestLogger _logger;
    public SampleJobHandler(JobHandlerTestLogger logger) => _logger = logger;
    public async Task Handle(TJob job, CancellationToken ct)
    {
        _logger.Log<TImpl>(job, JobHandlerStep.Starting);
        await Task.Delay(TimeSpan.FromSeconds(1), ct);
        _logger.Log<TImpl>(job, JobHandlerStep.SuccessfullyCompleted);
    }
}

class SampleJobHandlerA1 : SampleJobHandler<SampleJobA, SampleJobHandlerA1>
{
    public SampleJobHandlerA1(JobHandlerTestLogger logger) : base(logger) { }
}

class SampleJobHandlerA2 : SampleJobHandler<SampleJobA, SampleJobHandlerA2>
{
    public SampleJobHandlerA2(JobHandlerTestLogger logger) : base(logger) { }
}

class SampleJobHandlerB : SampleJobHandler<SampleJobB, SampleJobHandlerB>
{
    public SampleJobHandlerB(JobHandlerTestLogger logger) : base(logger) { }
}

public class BakhooWorkerTests
{
    private readonly ITestOutputHelper _output;

    private readonly ServiceCollection _services;

	public BakhooWorkerTests(ITestOutputHelper output)
	{
        _output = output;

        var dbName = Utilities.CreateTestDbName();

        _services = new ServiceCollection();
        ConfigureBakhooWithMocks();
	}

    private void ConfigureBakhooWithMocks()
	{
        _services.AddScoped<BakhooWorker>();
        _services.AddTestLogging(_output);

        _services.AddMockedScoped<IBakhooJobStateService>((provider, mock) =>
        {
            mock.Setup(x => x.GetJobAsync(It.IsAny<Guid>(), ItIs.CT()))
                .ReturnsAsync((Func<Guid, CancellationToken, BakhooJob>)(
                    (id, ct) => provider.GetRequiredService<List<BakhooJob>>()
                        .First(y => y.Id == id)));
        });

        _services.AddMockedScoped<IBakhooJobStateObserver>(mock => { });

        _services.AddSingleton<JobHandlerTestLogger>();
        _services.AddTransient<IBakhooJobHandler, SampleJobHandlerA1>();
        _services.AddTransient<IBakhooJobHandler, SampleJobHandlerA2>();
        _services.AddTransient<IBakhooJobHandler, SampleJobHandlerB>();
    }

    [Fact]
	public async Task TestStartWorker()
	{
        var job1 = new BakhooJob
        {
            Id = Guid.NewGuid(),
            Type = typeof(SampleJobA).FullName,
            Data = JsonSerializer.Serialize(new SampleJobA {}),
        };
        var fakeJobs = new List<BakhooJob> { job1 };
        _services.AddSingleton<List<BakhooJob>>(fakeJobs);

        using var provider = _services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var cts = new CancellationTokenSource();

		var worker = scope.ServiceProvider.GetRequiredService<BakhooWorker>();
		await worker.StartWorkerAsync(job1.Id, cts.Token);

        var jobStateMock = scope.ServiceProvider.GetRequiredService<Mock<IBakhooJobStateService>>();
        jobStateMock.Verify(x
            => x.StartJobAsync(
                It.Is<Guid>(id => id == job1.Id), ItIs.CT()),
                Times.Once);

        var mockJobStateObserver = scope.ServiceProvider.GetRequiredService<Mock<IBakhooJobStateObserver>>();
        mockJobStateObserver.Verify(x
            => x.NotifyIssueImportJobUpdatedAsync(
                It.Is<Guid>(id => id == job1.Id), ItIs.CT()),
                Times.Once);

        Assert.NotNull(worker.RunTask);
        //Assert.False(worker.RunTask.IsCompleted);

        // wait until the job gets done
        await worker.RunTask;

        Assert.False(worker.RunTask.IsCanceled);
        Assert.True(worker.RunTask.IsCompletedSuccessfully);
        Assert.Null(worker.CancelationTask);

        jobStateMock.Verify(x => x.UpdateSuccessfulJobStateAsync(It.IsAny<Guid>(), ItIs.CT()));

        var monitor = scope.ServiceProvider.GetRequiredService<JobHandlerTestLogger>();
        Assert.Collection(monitor.Logs.Where(x => x.JobHandlerType == typeof(SampleJobHandlerA1)),
            new Action<JobHandlerLog>[]
            {
                log => Assert.Equal(JobHandlerStep.Starting, log.Step),
                log => Assert.Equal(JobHandlerStep.SuccessfullyCompleted, log.Step),
            });
        Assert.Collection(monitor.Logs.Where(x => x.JobHandlerType == typeof(SampleJobHandlerA2)),
            new Action<JobHandlerLog>[]
            {
                log => Assert.Equal(JobHandlerStep.Starting, log.Step),
                log => Assert.Equal(JobHandlerStep.SuccessfullyCompleted, log.Step),
            });
        Assert.Collection(monitor.Logs.Where(x => x.JobHandlerType == typeof(SampleJobHandlerB)),
            new Action<JobHandlerLog>[] { });

        mockJobStateObserver.Verify(x
            => x.NotifyIssueImportJobUpdatedAsync(
                It.Is<Guid>(id => id == job1.Id), ItIs.CT()),
                Times.Exactly(2));
    }
}
