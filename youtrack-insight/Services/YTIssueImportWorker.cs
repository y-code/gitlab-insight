using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using YouTrackInsight.Domain;
using YouTrackInsight.Entity;

namespace YouTrackInsight.Services
{
    public class YTIssueImportWorker
    {
        private YTInsightDbContext _db;
        private readonly YTIssueImportService _importService;
        private readonly YouTrackInsightOptions _options;

        public Task Task { get; private set; }

        public YTIssueImportWorker(YTInsightDbContext db, YTIssueImportService importService, IOptions<YouTrackInsightOptions> options)
        {
            _importService = importService;
            _options = options.Value;
        }

        public async Task RunAsync(Guid taskId, CancellationToken ct)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
        }
    }
}

