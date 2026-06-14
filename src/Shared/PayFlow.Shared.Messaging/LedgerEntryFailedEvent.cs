namespace PayFlow.Shared.Messaging;

public record LedgerEntryFailedEvent : IntegrationEvent
{
    public string ErrorCode { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
}
