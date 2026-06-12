using LedgerService.Domain;

namespace LedgerService.Tests;

public class AccountIdTests
{
    [Fact]
    public void Constructor_AssignsValue()
    {
        var guid = Guid.NewGuid();
        var id = new AccountId(guid);
        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void New_CreatesNonEmptyGuid()
    {
        var id = AccountId.New();
        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void New_ReturnsDifferentValues()
    {
        var id1 = AccountId.New();
        var id2 = AccountId.New();
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var guid = Guid.NewGuid();
        var id1 = new AccountId(guid);
        var id2 = new AccountId(guid);
        Assert.Equal(id1, id2);
    }

    [Fact]
    public void ToString_ContainsValue()
    {
        var guid = Guid.NewGuid();
        var id = new AccountId(guid);
        Assert.Contains(guid.ToString(), id.ToString());
    }
}

public class TransactionIdTests
{
    [Fact]
    public void Constructor_AssignsValue()
    {
        var guid = Guid.NewGuid();
        var id = new TransactionId(guid);
        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void New_CreatesNonEmptyGuid()
    {
        var id = TransactionId.New();
        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void New_ReturnsDifferentValues()
    {
        var id1 = TransactionId.New();
        var id2 = TransactionId.New();
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var guid = Guid.NewGuid();
        var id1 = new TransactionId(guid);
        var id2 = new TransactionId(guid);
        Assert.Equal(id1, id2);
    }

    [Fact]
    public void ToString_ContainsValue()
    {
        var guid = Guid.NewGuid();
        var id = new TransactionId(guid);
        Assert.Contains(guid.ToString(), id.ToString());
    }
}

public class EntryTypeTests
{
    [Fact]
    public void Debit_HasValueZero()
    {
        Assert.Equal(0, (int)EntryType.Debit);
    }

    [Fact]
    public void Credit_HasValueOne()
    {
        Assert.Equal(1, (int)EntryType.Credit);
    }

    [Fact]
    public void EntryType_CanParseFromString()
    {
        Assert.True(Enum.TryParse<EntryType>("Debit", true, out var debit));
        Assert.Equal(EntryType.Debit, debit);

        Assert.True(Enum.TryParse<EntryType>("Credit", true, out var credit));
        Assert.Equal(EntryType.Credit, credit);
    }
}

public class LedgerErrorsTests
{
    [Fact]
    public void InsufficientFunds_HasConflictCode()
    {
        var error = LedgerErrors.InsufficientFunds;
        Assert.Equal("CONFLICT", error.Code);
        Assert.Contains("Insufficient funds", error.Message);
    }

    [Fact]
    public void AccountNotFound_HasNotFoundCode()
    {
        var error = LedgerErrors.AccountNotFound;
        Assert.Equal("NOT_FOUND", error.Code);
        Assert.Contains("Account not found", error.Message);
    }

    [Fact]
    public void InvalidEntry_HasValidationCode()
    {
        var error = LedgerErrors.InvalidEntry("test detail");
        Assert.Equal("VALIDATION", error.Code);
        Assert.Contains("test detail", error.Message);
    }

    [Fact]
    public void InvalidEntry_GeneratesUniqueMessages()
    {
        var error1 = LedgerErrors.InvalidEntry("first");
        var error2 = LedgerErrors.InvalidEntry("second");
        Assert.NotEqual(error1.Message, error2.Message);
    }
}