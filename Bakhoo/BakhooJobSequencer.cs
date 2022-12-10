using System;
using Bakhoo.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bakhoo;

internal interface IBakhooJobSequencer
{
    IAsyncEnumerable<Guid> GetJobsInBacklogAsync();
}

internal class BakhooJobSequencerSlim : IBakhooJobSequencer
{
    private readonly IServiceProvider _provider;
    private readonly BakhooOptions _options;

    public BakhooJobSequencerSlim(
        IServiceProvider provider,
        IOptions<BakhooOptions> options)
    {
        _provider = provider;
        _options = options.Value;
    }

    public async IAsyncEnumerable<Guid> GetJobsInBacklogAsync()
    {
        Guid[] jobIds;

        using (var scope = _provider.CreateScope())
        {
            BakhooDbContext _db = scope.ServiceProvider.GetRequiredService<BakhooDbContext>();
            jobIds = await _db.Jobs
                .Where(x => !x.Start.HasValue)
                .OrderBy(x => x.Submitted)
                .Select(x => x.Id)
                .ToAsyncEnumerable()
                .ToArrayAsync();
        }

        foreach (var jobId in jobIds)
            yield return jobId;
    }
}
