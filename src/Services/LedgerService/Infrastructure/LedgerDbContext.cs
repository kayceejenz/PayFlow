using Microsoft.EntityFrameworkCore;
using LedgerService.Domain;

namespace LedgerService.Infrastructure;

public class LedgerDbContext : DbContext
{
    public LedgerDbContext(DbContextOptions<LedgerDbContext> options) : base(options) { }

    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LedgerEntry>(entity =>
        {
            entity.ToTable("ledger_entries");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.TransactionId).IsRequired();
            entity.Property(e => e.AccountId).IsRequired();
            entity.Property(e => e.EntryType).IsRequired().HasConversion<string>();
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.Currency).HasMaxLength(3).IsRequired();
            entity.Property(e => e.CreatedAtUtc).IsRequired();
            entity.Property(e => e.Reference).HasMaxLength(255);
            entity.Property(e => e.Metadata).HasColumnType("jsonb");

            entity.HasIndex(e => e.AccountId);
            entity.HasIndex(e => e.TransactionId);
            entity.HasIndex(e => new { e.AccountId, e.CreatedAtUtc });
        });
    }
}

public class LedgerEntry
{
    public Guid Id { get; init; }
    public Guid TransactionId { get; init; }
    public Guid AccountId { get; init; }
    public EntryType EntryType { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "GBP";
    public DateTime CreatedAtUtc { get; init; }
    public string? Reference { get; init; }
    public string? Metadata { get; init; }
}
