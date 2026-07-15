namespace StatementService.Domain;

public class StatementEntry
{
    public Guid Id { get; init; }
    public required Guid WalletId { get; init; }
    public required Guid TransactionId { get; init; }
    public required string EntryType { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required Guid CounterpartyId { get; init; }
    public string? Reference { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
}
