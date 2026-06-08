namespace PayFlow.Shared.Messaging;

public interface IOutboxMessage
{
    Guid Id { get; }
    string Type { get; }
    string Payload { get; }
    DateTime CreatedAtUtc { get; }
    DateTime? ProcessedAtUtc { get; }
    int RetryCount { get; }
}
