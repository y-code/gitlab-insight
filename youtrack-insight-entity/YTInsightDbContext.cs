using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace YouTrackInsight.Entity;

public class YTInsightDbContext : DbContext
{
    public DbSet<YTIssueModel> Issues { get; private set; }
    public DbSet<YTIssueLinkModel> IssueLinks { get; private set; }
    public DbSet<YTIssueImportTask> IssueImportTasks { get; private set; }

    public YTInsightDbContext(DbContextOptions<YTInsightDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<YTIssueModel>();
        modelBuilder.Entity<YTIssueLinkModel>();
        modelBuilder.Entity<YTIssueImportTask>();
    }
}
