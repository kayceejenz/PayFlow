using StatementService.Domain;

namespace StatementService.Features.GetStatements;

public record GetStatementsQuery(Guid WalletId, int Page, int PageSize);

public record StatementEntryResponse(
    Guid Id,
    Guid TransactionId,
    string EntryType,
    decimal Amount,
    string Currency,
    Guid CounterpartyId,
    string? Reference,
    DateTime CreatedAtUtc);

public record GetStatementsResponse(
    Guid WalletId,
    IReadOnlyList<StatementEntryResponse> Entries,
    int Page,
    int PageSize,
    int TotalCount);
