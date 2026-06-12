using System.Diagnostics;
using LedgerService.Domain;
using LedgerService.Infrastructure;
using PayFlow.Shared.Observability;
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
        using var activity = Telemetry.ActivitySource.StartActivity("GetBalance");
        activity?.SetTag("account.id", query.AccountId);

        var entries = await _repository.GetEntriesByAccountIdAsync(query.AccountId, ct);

        if (entries.Count == 0)
            return Result.Failure<GetBalanceResponse>(LedgerErrors.AccountNotFound);

        var credits = entries.Where(e => e.EntryType == EntryType.Credit).Sum(e => e.Amount);
        var debits = entries.Where(e => e.EntryType == EntryType.Debit).Sum(e => e.Amount);
        var balance = credits - debits;
        var currency = entries[0].Currency;

        activity?.SetTag("balance", balance);
        activity?.SetTag("currency", currency);
        activity?.SetTag("entry.count", entries.Count);

        return Result.Success(new GetBalanceResponse(query.AccountId, balance, currency));
    }
}
