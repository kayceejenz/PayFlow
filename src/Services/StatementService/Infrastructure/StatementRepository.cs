using Microsoft.EntityFrameworkCore;
using StatementService.Domain;

namespace StatementService.Infrastructure;

public interface IStatementRepository
{
    Task<int> GetTotalCountAsync(Guid walletId, CancellationToken ct);
    Task<IReadOnlyList<StatementEntry>> GetPagedAsync(Guid walletId, int skip, int take, CancellationToken ct);
}

public class StatementRepository : IStatementRepository
{
    private readonly StatementDbContext _db;

    public StatementRepository(StatementDbContext db)
    {
        _db = db;
    }

    public async Task<int> GetTotalCountAsync(Guid walletId, CancellationToken ct)
    {
        return await _db.StatementEntries
            .Where(e => e.WalletId == walletId)
            .CountAsync(ct);
    }

    public async Task<IReadOnlyList<StatementEntry>> GetPagedAsync(
        Guid walletId, int skip, int take, CancellationToken ct)
    {
        return await _db.StatementEntries
            .Where(e => e.WalletId == walletId)
            .OrderByDescending(e => e.CreatedAtUtc)
            .ThenBy(e => e.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }
}
