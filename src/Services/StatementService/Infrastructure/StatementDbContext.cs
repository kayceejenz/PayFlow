using Microsoft.EntityFrameworkCore;
using StatementService.Domain;

namespace StatementService.Infrastructure;

public class StatementDbContext : DbContext
{
    public StatementDbContext(DbContextOptions<StatementDbContext> options) : base(options) { }

    public DbSet<StatementEntry> StatementEntries => Set<StatementEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StatementEntry>(entity =>
        {
            entity.ToTable("statement_entries");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.WalletId).IsRequired();
            entity.Property(e => e.TransactionId).IsRequired();
            entity.Property(e => e.EntryType).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.Currency).HasMaxLength(3).IsRequired();
            entity.Property(e => e.CounterpartyId).IsRequired();
            entity.Property(e => e.Reference).HasMaxLength(255);
            entity.Property(e => e.CreatedAtUtc).IsRequired();

            entity.HasIndex(e => e.WalletId);
            entity.HasIndex(e => new { e.WalletId, e.CreatedAtUtc });
        });
    }
}
