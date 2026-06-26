using PaymentService.Domain;

namespace PaymentService.Infrastructure;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _dbContext;

    public PaymentRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Payment payment, CancellationToken ct)
    {
        await _dbContext.Payments.AddAsync(payment, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _dbContext.Payments.FindAsync([id], ct);
    }

    public async Task UpdateAsync(Payment payment, CancellationToken ct)
    {
        _dbContext.Payments.Update(payment);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task SaveOutboxMessageAsync(OutboxMessage message, CancellationToken ct)
    {
        await _dbContext.OutboxMessages.AddAsync(message, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<OutboxMessage>> GetUnprocessedOutboxMessagesAsync(int batchSize, CancellationToken ct)
    {
        return await _dbContext.OutboxMessages
            .Where(m => m.ProcessedAtUtc == null)
            .OrderBy(m => m.CreatedAtUtc)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public async Task MarkOutboxMessageProcessedAsync(Guid id, CancellationToken ct)
    {
        var message = await _dbContext.OutboxMessages.FindAsync([id], ct);
        if (message != null)
        {
            message.ProcessedAtUtc = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
