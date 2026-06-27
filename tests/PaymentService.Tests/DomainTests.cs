using PaymentService.Domain;

namespace PaymentService.Tests;

public class PaymentIdTests
{
    [Fact]
    public void Constructor_AssignsValue()
    {
        var guid = Guid.NewGuid();
        var id = new PaymentId(guid);
        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void New_CreatesNonEmptyGuid()
    {
        var id = PaymentId.New();
        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void New_ReturnsDifferentValues()
    {
        var id1 = PaymentId.New();
        var id2 = PaymentId.New();
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var guid = Guid.NewGuid();
        var id1 = new PaymentId(guid);
        var id2 = new PaymentId(guid);
        Assert.Equal(id1, id2);
    }
}

public class PaymentStatusTests
{
    [Fact]
    public void PendingAuthorization_HasValueZero()
    {
        Assert.Equal(0, (int)PaymentStatus.PendingAuthorization);
    }

    [Fact]
    public void Authorized_HasValueOne()
    {
        Assert.Equal(1, (int)PaymentStatus.Authorized);
    }

    [Fact]
    public void ProcessingCapture_HasValueTwo()
    {
        Assert.Equal(2, (int)PaymentStatus.ProcessingCapture);
    }

    [Fact]
    public void Captured_HasValueThree()
    {
        Assert.Equal(3, (int)PaymentStatus.Captured);
    }

    [Fact]
    public void ProcessingRelease_HasValueFour()
    {
        Assert.Equal(4, (int)PaymentStatus.ProcessingRelease);
    }

    [Fact]
    public void Released_HasValueFive()
    {
        Assert.Equal(5, (int)PaymentStatus.Released);
    }

    [Fact]
    public void Failed_HasValueSix()
    {
        Assert.Equal(6, (int)PaymentStatus.Failed);
    }

    [Fact]
    public void CanParseFromString()
    {
        Assert.True(Enum.TryParse<PaymentStatus>("PendingAuthorization", true, out var pending));
        Assert.Equal(PaymentStatus.PendingAuthorization, pending);

        Assert.True(Enum.TryParse<PaymentStatus>("Authorized", true, out var authorized));
        Assert.Equal(PaymentStatus.Authorized, authorized);

        Assert.True(Enum.TryParse<PaymentStatus>("Captured", true, out var captured));
        Assert.Equal(PaymentStatus.Captured, captured);

        Assert.True(Enum.TryParse<PaymentStatus>("Released", true, out var released));
        Assert.Equal(PaymentStatus.Released, released);

        Assert.True(Enum.TryParse<PaymentStatus>("Failed", true, out var failed));
        Assert.Equal(PaymentStatus.Failed, failed);
    }
}

public class PaymentErrorsTests
{
    [Fact]
    public void NotFound_HasNotFoundCode()
    {
        var error = PaymentErrors.NotFound;
        Assert.Equal("NOT_FOUND", error.Code);
        Assert.Contains("not found", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NotAuthorized_HasConflictCode()
    {
        var error = PaymentErrors.NotAuthorized;
        Assert.Equal("CONFLICT", error.Code);
        Assert.Contains("not in Authorized", error.Message);
    }

    [Fact]
    public void AlreadyCaptured_HasConflictCode()
    {
        var error = PaymentErrors.AlreadyCaptured;
        Assert.Equal("CONFLICT", error.Code);
        Assert.Contains("already been captured", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AlreadyReleased_HasConflictCode()
    {
        var error = PaymentErrors.AlreadyReleased;
        Assert.Equal("CONFLICT", error.Code);
        Assert.Contains("already been released", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void HoldsAccount_HasExpectedId()
    {
        Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000001"), HoldsAccount.AccountId);
    }
}

public class PaymentTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var now = DateTime.UtcNow;
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            PayerAccountId = Guid.NewGuid(),
            MerchantAccountId = Guid.NewGuid(),
            Amount = 100.50m,
            Currency = "GBP",
            Status = PaymentStatus.PendingAuthorization,
            Reference = "test-payment",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        Assert.Equal(100.50m, payment.Amount);
        Assert.Equal("GBP", payment.Currency);
        Assert.Equal(PaymentStatus.PendingAuthorization, payment.Status);
        Assert.Equal("test-payment", payment.Reference);
    }

    [Fact]
    public void StatusTransitions_Allowed()
    {
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

        payment.Status = PaymentStatus.Authorized;
        Assert.Equal(PaymentStatus.Authorized, payment.Status);

        payment.Status = PaymentStatus.ProcessingCapture;
        Assert.Equal(PaymentStatus.ProcessingCapture, payment.Status);

        payment.Status = PaymentStatus.Captured;
        Assert.Equal(PaymentStatus.Captured, payment.Status);
    }
}
