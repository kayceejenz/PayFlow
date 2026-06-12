namespace LedgerService.Features.CreateEntry;

public record CreateEntryCommand
{
    public Guid TransactionId { get; init; }
    public Guid AccountId { get; init; }
    public string EntryType { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "GBP";
    public string? Reference { get; init; }
}

public record CreateEntryResponse(Guid EntryId);

public record CreateEntryPairCommand
{
    public Guid DebitAccountId { get; init; }
    public Guid CreditAccountId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "GBP";
    public string? Reference { get; init; }
}

public record CreateEntryPairResponse(Guid TransactionId, Guid DebitEntryId, Guid CreditEntryId);