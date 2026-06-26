using PaymentService.Domain;

namespace PaymentService.Infrastructure;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken ct);
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct);
    Task UpdateAsync(Payment payment, CancellationToken ct);
    Task SaveOutboxMessageAsync(OutboxMessage message, CancellationToken ct);
    Task<List<OutboxMessage>> GetUnprocessedOutboxMessagesAsync(int batchSize, CancellationToken ct);
    Task MarkOutboxMessageProcessedAsync(Guid id, CancellationToken ct);
}
