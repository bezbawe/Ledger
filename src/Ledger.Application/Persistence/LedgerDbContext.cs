using Ledger.EventStore;
using Ledger.Projections;
using Microsoft.EntityFrameworkCore;

namespace Ledger.Application.Persistence;

public sealed class LedgerDbContext : DbContext
{
    public LedgerDbContext(DbContextOptions<LedgerDbContext> options) : base(options)
    {
    }

    public DbSet<EventRecord> Events => Set<EventRecord>();
    public DbSet<AccountBalance> AccountBalances => Set<AccountBalance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventRecord>(e =>
        {
            e.HasIndex(x => new { x.StreamId, x.Version }).IsUnique();
            e.Property(x => x.EventType).IsRequired();
            e.Property(x => x.Data).IsRequired();
        });

        modelBuilder.Entity<AccountBalance>(e =>
        {
            e.HasIndex(x => x.AccountId).IsUnique();
        });
    }
}
