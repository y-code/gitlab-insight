using System;
using System.Text.Json;
using Bakhoo.Entity;
using Bakhoo.Entity.Test;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bakhoo.Test;

public class BakhooServiceTests : IDisposable
{
	private class JobInfo { }

	private readonly ServiceProvider _provider;

	public BakhooServiceTests()
	{
        var dbName = Utilities.CreateTestDbName();

		var services = new ServiceCollection();
		services.AddBakhooEntity(dbName);
        services.AddScoped<IBakhooJobStateService, BakhooJobStateService>();
		IConfiguration config = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["Logging:LogLevel:Default"] = $"{LogLevel.Debug}",
			})
			.Build();
		services.AddOptions<BakhooOptions>().Bind(config);
		services.AddDbContext<BakhooDbContext>(options =>
		{
			options.UseNpgsql(
				"User ID=postgres;" +
				"Password=test;" +
				"Host=localhost;" +
				"Port=5432;" +
				$"Database={dbName};" +
				"Pooling=true;" +
				"MinPoolSize=0;" +
				"MaxPoolSize=8;" +
				"Connection Lifetime=0;",
				x => x.MigrationsAssembly("YouTrackInsight"));
		});

		_provider = services.BuildServiceProvider();

        _provider.CreateTestDb<BakhooDbContext>();
	}

	public void Dispose()
	{
		_provider.DeleteTestDb<BakhooDbContext>();
        _provider.Dispose();
	}

	private async Task SetUpJobsAsync(params BakhooJob[] jobs)
	{
        using (var scope = _provider.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<BakhooDbContext>();
            await dbContext.Jobs.AddRangeAsync(jobs);
            await dbContext.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task TestSubmitJob()
    {
        var testStart = DateTimeOffset.UtcNow;
        var job1Id = Guid.NewGuid();
		var job1Data = new JobInfo();

        using (var scope = _provider.CreateAsyncScope())
        {
			var service = scope.ServiceProvider
				.GetRequiredService<IBakhooJobStateService>();
			var cts = new CancellationTokenSource();
			await service.SubmitJobAsync(job1Id, job1Data, cts.Token);
        }

		using (var scope = _provider.CreateAsyncScope())
		{
            var dbContext = scope.ServiceProvider.GetRequiredService<BakhooDbContext>();
            var currentJob = dbContext.Jobs.Single(x => x.Id == job1Id);
			Assert.Equal(currentJob.Type, job1Data.GetType().FullName);
            Assert.NotNull(currentJob.Submitted);
            Assert.InRange(currentJob.Submitted.Value, testStart, DateTimeOffset.Now);
            Assert.Null(currentJob.Start);
            Assert.Null(currentJob.End);
			Assert.False(currentJob.HasError);
			Assert.Null(currentJob.CancelRequested);
            Assert.False(currentJob.IsCancelling);
			Assert.False(currentJob.IsCancelled);
        }
    }

    [Fact]
    public async Task TestStartJob()
    {
        var job1Data = new JobInfo();
        var job1 = new BakhooJob
        {
            Id = Guid.NewGuid(),
            Type = job1Data.GetType().FullName,
            Submitted = DateTimeOffset.UtcNow,
            Data = JsonSerializer.Serialize(job1Data),
        };

        await SetUpJobsAsync(job1);

        using (var scope = _provider.CreateAsyncScope())
        {
            var service = scope.ServiceProvider
                .GetRequiredService<IBakhooJobStateService>();
            var cts = new CancellationTokenSource();
            await service.StartJobAsync(job1.Id, cts.Token);
        }

        using (var scope = _provider.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<BakhooDbContext>();
            var currentJob = dbContext.Jobs.Single(x => x.Id == job1.Id);
            Assert.Equal(currentJob.Type, job1.Type);
            Assert.NotNull(currentJob.Submitted);
            Assert.Equal(currentJob.Submitted, job1.Submitted);
            Assert.NotNull(currentJob.Start);
            Assert.InRange(currentJob.Start.Value, currentJob.Submitted.Value, DateTimeOffset.UtcNow);
            Assert.Null(currentJob.End);
            Assert.False(currentJob.HasError);
            Assert.Null(currentJob.CancelRequested);
            Assert.False(currentJob.IsCancelling);
            Assert.False(currentJob.IsCancelled);
        }
    }
}
