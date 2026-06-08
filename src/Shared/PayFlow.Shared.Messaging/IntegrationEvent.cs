namespace PayFlow.Shared.Messaging;

public abstract record IntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = string.Empty;
}
