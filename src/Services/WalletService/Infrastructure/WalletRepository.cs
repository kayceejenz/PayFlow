using WalletService.Domain;

namespace WalletService.Infrastructure;

public class WalletRepository : IWalletRepository
{
    private readonly WalletDbContext _dbContext;

    public WalletRepository(WalletDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Wallet wallet, CancellationToken ct)
    {
        await _dbContext.Wallets.AddAsync(wallet, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<Wallet?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _dbContext.Wallets.FindAsync([id], ct);
    }

    public async Task<Wallet?> GetByAccountIdAsync(Guid accountId, CancellationToken ct)
    {
        return await _dbContext.Wallets
            .FirstOrDefaultAsync(w => w.AccountId == accountId, ct);
    }

    public async Task UpdateAsync(Wallet wallet, CancellationToken ct)
    {
        _dbContext.Wallets.Update(wallet);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task SaveOutboxMessageAsync(OutboxMessage message, CancellationToken ct)
    {
        await _dbContext.OutboxMessages.AddAsync(message, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<OutboxMessage>> GetUnprocessedOutboxMessagesAsync(int batchSize, CancellationToken ct)
    {
        return await _dbContext.OutboxMessages
            .Where(m => m.ProcessedAtUtc == null)
            .OrderBy(m => m.CreatedAtUtc)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public async Task MarkOutboxMessageProcessedAsync(Guid id, CancellationToken ct)
    {
        var message = await _dbContext.OutboxMessages.FindAsync([id], ct);
        if (message != null)
        {
            message.ProcessedAtUtc = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
