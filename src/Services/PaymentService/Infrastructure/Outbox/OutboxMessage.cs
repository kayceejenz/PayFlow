using PayFlow.Shared.Messaging;

namespace PaymentService.Infrastructure;

public class OutboxMessage : IOutboxMessage
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
    public Guid CorrelationId { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? ProcessedAtUtc { get; set; }
    public int RetryCount { get; set; }
}
