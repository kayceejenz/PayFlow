using System.Diagnostics;
using LedgerService.Domain;
using LedgerService.Infrastructure;
using PayFlow.Shared.Observability;
using PayFlow.Shared.Primitives;

namespace LedgerService.Features.GetTransactionHistory;

public class GetTransactionHistoryHandler
{
    private readonly ILedgerRepository _repository;

    public GetTransactionHistoryHandler(ILedgerRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<GetTransactionHistoryResponse>> HandleAsync(
        GetTransactionHistoryQuery query, CancellationToken ct)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("GetTransactionHistory");
        activity?.SetTag("account.id", query.AccountId);

        var entries = await _repository.GetEntriesByAccountIdAsync(query.AccountId, ct);

        if (entries.Count == 0)
            return Result.Failure<GetTransactionHistoryResponse>(LedgerErrors.AccountNotFound);

        var transactionEntries = entries.Select(e => new TransactionEntry(
            e.Id,
            e.TransactionId,
            e.EntryType.ToString(),
            e.Amount,
            e.Currency,
            e.CreatedAtUtc,
            e.Reference
        )).ToList();

        activity?.SetTag("entry.count", transactionEntries.Count);

        return Result.Success(new GetTransactionHistoryResponse(query.AccountId, transactionEntries));
    }
}
