using Microsoft.Extensions.Options;
using YouTrackInsight.Domain;
using YouTrackInsight.Entity;

namespace YouTrackInsight.Services;
public class YTIssueImportManager : BackgroundService
{
    private record WorkerContext(
        IServiceScope Scope,
        Guid ImportTaskId,
        YTIssueImportWorker Worker,
        Task? Task);

    private readonly IServiceProvider _serviceProvider;
    private readonly YouTrackInsightOptions _options;

    private List<WorkerContext> _workerContexts = new();

    public YTIssueImportManager(
        IServiceProvider serviceProvider,
        IOptions<YouTrackInsightOptions> options)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));

            lock (_workerContexts)
            {
                foreach (var workerContext in _workerContexts.ToArray())
                {
                    if (workerContext.Task == null || workerContext.Task.IsCompleted)
                    {
                        workerContext.Scope.Dispose();
                        _workerContexts.Remove(workerContext);
                    }
                }

                if (_workerContexts.Count >= _options.IssueImport.MaxParallelTasks)
                    continue;
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var importService = scope.ServiceProvider.GetRequiredService<YTIssueImportService>();
                var tasks = importService.GetTasksInBacklogAsync();

                await foreach (var importTask in tasks)
                {
                    if (ct.IsCancellationRequested)
                        return;
                    if (_workerContexts.Count >= _options.IssueImport.MaxParallelTasks) // TODO: check with the table in DB
                        break;

                    await StartWorderAsync(importTask.Id, ct);
                }
            }
        }
    }

    public async Task StartWorderAsync(Guid taskId, CancellationToken ct)
    {
        var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<YTInsightDbContext>();

        var importService = scope.ServiceProvider.GetRequiredService<YTIssueImportService>();
        var hubClients = scope.ServiceProvider.GetRequiredService<YouTrackInsightHubClients>();

        var worker = scope.ServiceProvider.GetRequiredService<YTIssueImportWorker>();

        Task? task = null;

        try
        {
            using (var tx = await db.Database.BeginTransactionAsync())
            {
                var importTask = await importService.GetTaskAsync(taskId, ct);

                if (importTask == null)
                    throw new ArgumentException($"There is no task with ID {taskId}");

                importTask.Start = DateTimeOffset.UtcNow;
                importTask.Message = "Issue import is in progress.";

                await db.SaveChangesAsync(ct);
                await tx.CommitAsync();
            }

            await hubClients.NotifyIssueImportTaskUpdatedAsync(taskId, ct);

            task = runAsync(worker, importService);
        }
        catch (Exception e)
        {
            await importService.UpdateFailedTaskStateAsync(taskId, e.Message, ct);
        }

        _workerContexts.Add(new WorkerContext(scope, taskId, worker, task));

        async Task runAsync(YTIssueImportWorker worker, YTIssueImportService importService)
        {
            await worker.RunAsync(taskId, ct);

            await importService.UpdateSuccessfulTaskStateAsync(taskId, ct);

            await hubClients.NotifyIssueImportTaskUpdatedAsync(taskId, ct);
        }
    }
}
