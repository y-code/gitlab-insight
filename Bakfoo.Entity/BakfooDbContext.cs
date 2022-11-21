using Microsoft.EntityFrameworkCore;

namespace Bakfoo.Entity;

public class BakfooDbContext : DbContext
{
    public DbSet<BakfooJob> Jobs { get; private set; }

    public BakfooDbContext(DbContextOptions<BakfooDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BakfooJob>();
    }
}
