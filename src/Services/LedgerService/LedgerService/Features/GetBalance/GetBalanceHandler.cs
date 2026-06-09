using LedgerService.Domain;
using LedgerService.Infrastructure;
using PayFlow.Shared.Primitives;

namespace LedgerService.Features.GetBalance;

public class GetBalanceHandler
{
    private readonly ILedgerRepository _repository;

    public GetBalanceHandler(ILedgerRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<GetBalanceResponse>> HandleAsync(GetBalanceQuery query, CancellationToken ct)
    {
        var entries = await _repository.GetEntriesByAccountIdAsync(query.AccountId, ct);

        if (entries.Count == 0)
            return Result.Failure<GetBalanceResponse>(LedgerErrors.AccountNotFound);

        var credits = entries.Where(e => e.EntryType == EntryType.Credit).Sum(e => e.Amount);
        var debits = entries.Where(e => e.EntryType == EntryType.Debit).Sum(e => e.Amount);
        var balance = credits - debits;
        var currency = entries[0].Currency;

        return Result.Success(new GetBalanceResponse(query.AccountId, balance, currency));
    }
}
