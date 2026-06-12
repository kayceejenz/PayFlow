using LedgerService.Infrastructure;

namespace LedgerService.Features.GetTransactionHistory;

public record GetTransactionHistoryQuery(Guid AccountId);

public record TransactionEntry(
    Guid EntryId,
    Guid TransactionId,
    string EntryType,
    decimal Amount,
    string Currency,
    DateTime CreatedAtUtc,
    string? Reference);

public record GetTransactionHistoryResponse(
    Guid AccountId,
    IReadOnlyList<TransactionEntry> Entries);
