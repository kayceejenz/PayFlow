namespace PayFlow.Shared.Messaging;

public record CreateLedgerEntryCommand
{
    public string CorrelationId { get; init; } = string.Empty;
    public Guid DebitAccountId { get; init; }
    public Guid CreditAccountId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "GBP";
    public string? Reference { get; init; }
}
