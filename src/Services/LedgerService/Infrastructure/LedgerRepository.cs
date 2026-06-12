using Microsoft.EntityFrameworkCore;

namespace LedgerService.Infrastructure;

public class LedgerRepository : ILedgerRepository
{
    private readonly LedgerDbContext _dbContext;

    public LedgerRepository(LedgerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddEntriesAsync(IEnumerable<LedgerEntry> entries, CancellationToken ct)
    {
        _dbContext.LedgerEntries.AddRange(entries);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<LedgerEntry>> GetEntriesByAccountIdAsync(Guid accountId, CancellationToken ct)
    {
        return await _dbContext.LedgerEntries
            .Where(e => e.AccountId == accountId)
            .OrderBy(e => e.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<LedgerEntry>> GetEntriesByTransactionIdAsync(Guid transactionId, CancellationToken ct)
    {
        return await _dbContext.LedgerEntries
            .Where(e => e.TransactionId == transactionId)
            .OrderBy(e => e.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<decimal> GetBalanceAsync(Guid accountId, CancellationToken ct)
    {
        var entries = await _dbContext.LedgerEntries
            .Where(e => e.AccountId == accountId)
            .ToListAsync(ct);

        var credits = entries.Where(e => e.EntryType == Domain.EntryType.Credit).Sum(e => e.Amount);
        var debits = entries.Where(e => e.EntryType == Domain.EntryType.Debit).Sum(e => e.Amount);

        return credits - debits;
    }
}
