using Microsoft.EntityFrameworkCore;
using PaymentService.Domain;
using PaymentService.Infrastructure;

namespace PaymentService.Tests;

public class PaymentDbContextTests
{
    private static PaymentDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new PaymentDbContext(options);
    }

    [Fact]
    public void Payments_TableExists()
    {
        using var db = CreateDbContext(Guid.NewGuid().ToString());
        Assert.NotNull(db.Payments);
    }

    [Fact]
    public void OutboxMessages_TableExists()
    {
        using var db = CreateDbContext(Guid.NewGuid().ToString());
        Assert.NotNull(db.OutboxMessages);
    }
}

public class PaymentRepositoryTests
{
    private static PaymentDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new PaymentDbContext(options);
    }

    [Fact]
    public async Task AddAsync_PersistsPayment()
    {
        using var db = CreateDbContext(Guid.NewGuid().ToString());
        var repository = new PaymentRepository(db);
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            PayerAccountId = Guid.NewGuid(),
            MerchantAccountId = Guid.NewGuid(),
            Amount = 100,
            Status = PaymentStatus.PendingAuthorization,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await repository.AddAsync(payment, CancellationToken.None);

        var saved = await db.Payments.FindAsync([payment.Id]);
        Assert.NotNull(saved);
        Assert.Equal(payment.Id, saved.Id);
        Assert.Equal(payment.PayerAccountId, saved.PayerAccountId);
        Assert.Equal(PaymentStatus.PendingAuthorization, saved.Status);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        using var db = CreateDbContext(Guid.NewGuid().ToString());
        var repository = new PaymentRepository(db);

        var result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsPayment_WhenFound()
    {
        using var db = CreateDbContext(Guid.NewGuid().ToString());
        var repository = new PaymentRepository(db);
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            PayerAccountId = Guid.NewGuid(),
            MerchantAccountId = Guid.NewGuid(),
            Amount = 100,
            Status = PaymentStatus.Authorized,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        db.Payments.Add(payment);
        await db.SaveChangesAsync();

        var result = await repository.GetByIdAsync(payment.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(payment.Id, result.Id);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesPayment()
    {
        using var db = CreateDbContext(Guid.NewGuid().ToString());
        var repository = new PaymentRepository(db);
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            PayerAccountId = Guid.NewGuid(),
            MerchantAccountId = Guid.NewGuid(),
            Amount = 100,
            Status = PaymentStatus.Authorized,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        db.Payments.Add(payment);
        await db.SaveChangesAsync();

        payment.Status = PaymentStatus.ProcessingCapture;
        payment.UpdatedAtUtc = DateTime.UtcNow;
        await repository.UpdateAsync(payment, CancellationToken.None);

        var saved = await db.Payments.FindAsync([payment.Id]);
        Assert.NotNull(saved);
        Assert.Equal(PaymentStatus.ProcessingCapture, saved.Status);
    }

    [Fact]
    public async Task SaveOutboxMessage_PersistsMessage()
    {
        using var db = CreateDbContext(Guid.NewGuid().ToString());
        var repository = new PaymentRepository(db);
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "TestType",
            Payload = "{}",
            CorrelationId = Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow,
            RetryCount = 0
        };

        await repository.SaveOutboxMessageAsync(message, CancellationToken.None);

        var saved = await db.OutboxMessages.FindAsync([message.Id]);
        Assert.NotNull(saved);
        Assert.Equal(message.Id, saved.Id);
        Assert.Equal("TestType", saved.Type);
    }

    [Fact]
    public async Task GetUnprocessedOutboxMessagesAsync_ReturnsOnlyUnprocessed()
    {
        using var db = CreateDbContext(Guid.NewGuid().ToString());
        var repository = new PaymentRepository(db);

        var processed = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "Processed",
            Payload = "{}",
            CorrelationId = Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-10),
            ProcessedAtUtc = DateTime.UtcNow,
            RetryCount = 0
        };
        var unprocessed = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "Unprocessed",
            Payload = "{}",
            CorrelationId = Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow,
            RetryCount = 0
        };
        db.OutboxMessages.AddRange(processed, unprocessed);
        await db.SaveChangesAsync();

        var results = await repository.GetUnprocessedOutboxMessagesAsync(10, CancellationToken.None);

        Assert.Single(results);
        Assert.Equal(unprocessed.Id, results[0].Id);
    }

    [Fact]
    public async Task MarkOutboxMessageProcessedAsync_SetsProcessedAt()
    {
        using var db = CreateDbContext(Guid.NewGuid().ToString());
        var repository = new PaymentRepository(db);
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "Test",
            Payload = "{}",
            CorrelationId = Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow,
            RetryCount = 0
        };
        db.OutboxMessages.Add(message);
        await db.SaveChangesAsync();

        await repository.MarkOutboxMessageProcessedAsync(message.Id, CancellationToken.None);

        var saved = await db.OutboxMessages.FindAsync([message.Id]);
        Assert.NotNull(saved);
        Assert.NotNull(saved.ProcessedAtUtc);
    }
}
