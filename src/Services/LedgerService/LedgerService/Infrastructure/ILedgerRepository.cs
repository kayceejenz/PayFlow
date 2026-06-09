namespace LedgerService.Infrastructure;

public interface ILedgerRepository
{
    Task AddEntriesAsync(IEnumerable<LedgerEntry> entries, CancellationToken ct);
    Task<IReadOnlyList<LedgerEntry>> GetEntriesByAccountIdAsync(Guid accountId, CancellationToken ct);
    Task<IReadOnlyList<LedgerEntry>> GetEntriesByTransactionIdAsync(Guid transactionId, CancellationToken ct);
    Task<decimal> GetBalanceAsync(Guid accountId, CancellationToken ct);
}
