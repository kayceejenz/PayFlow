namespace PayFlow.Shared.Messaging;

public record LedgerEntryCreatedEvent : IntegrationEvent
{
    public Guid TransactionId { get; init; }
    public Guid DebitEntryId { get; init; }
    public Guid CreditEntryId { get; init; }
    public Guid DebitAccountId { get; init; }
    public Guid CreditAccountId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
}
