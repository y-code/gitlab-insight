using Microsoft.EntityFrameworkCore;

namespace Bakhoo.Entity;

public class BakhooDbContext : DbContext
{
    public DbSet<BakhooJob> Jobs { get; private set; }

    public BakhooDbContext(DbContextOptions<BakhooDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BakhooJob>();
    }
}
