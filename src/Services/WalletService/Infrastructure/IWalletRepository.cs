using WalletService.Domain;

namespace WalletService.Infrastructure;

public interface IWalletRepository
{
    Task AddAsync(Wallet wallet, CancellationToken ct);
    Task<Wallet?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Wallet?> GetByAccountIdAsync(Guid accountId, CancellationToken ct);
    Task UpdateAsync(Wallet wallet, CancellationToken ct);
    Task SaveOutboxMessageAsync(OutboxMessage message, CancellationToken ct);
    Task<List<OutboxMessage>> GetUnprocessedOutboxMessagesAsync(int batchSize, CancellationToken ct);
    Task MarkOutboxMessageProcessedAsync(Guid id, CancellationToken ct);
}
