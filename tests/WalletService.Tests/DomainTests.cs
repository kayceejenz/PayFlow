using WalletService.Domain;

namespace WalletService.Tests;

public class WalletIdTests
{
    [Fact]
    public void Constructor_AssignsValue()
    {
        var guid = Guid.NewGuid();
        var id = new WalletId(guid);
        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void New_CreatesNonEmptyGuid()
    {
        var id = WalletId.New();
        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void New_ReturnsDifferentValues()
    {
        var id1 = WalletId.New();
        var id2 = WalletId.New();
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var guid = Guid.NewGuid();
        var id1 = new WalletId(guid);
        var id2 = new WalletId(guid);
        Assert.Equal(id1, id2);
    }
}

public class WalletStatusTests
{
    [Fact]
    public void Active_HasValueZero()
    {
        Assert.Equal(0, (int)WalletStatus.Active);
    }

    [Fact]
    public void Frozen_HasValueOne()
    {
        Assert.Equal(1, (int)WalletStatus.Frozen);
    }

    [Fact]
    public void Closed_HasValueTwo()
    {
        Assert.Equal(2, (int)WalletStatus.Closed);
    }

    [Fact]
    public void CanParseFromString()
    {
        Assert.True(Enum.TryParse<WalletStatus>("Active", true, out var active));
        Assert.Equal(WalletStatus.Active, active);

        Assert.True(Enum.TryParse<WalletStatus>("Frozen", true, out var frozen));
        Assert.Equal(WalletStatus.Frozen, frozen);

        Assert.True(Enum.TryParse<WalletStatus>("Closed", true, out var closed));
        Assert.Equal(WalletStatus.Closed, closed);
    }
}

public class WalletErrorsTests
{
    [Fact]
    public void NotFound_HasNotFoundCode()
    {
        var error = WalletErrors.NotFound;
        Assert.Equal("NOT_FOUND", error.Code);
        Assert.Contains("not found", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AlreadyFrozen_HasConflictCode()
    {
        var error = WalletErrors.AlreadyFrozen;
        Assert.Equal("CONFLICT", error.Code);
        Assert.Contains("already frozen", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AlreadyClosed_HasConflictCode()
    {
        var error = WalletErrors.AlreadyClosed;
        Assert.Equal("CONFLICT", error.Code);
        Assert.Contains("already closed", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Frozen_HasConflictCode()
    {
        var error = WalletErrors.Frozen;
        Assert.Equal("CONFLICT", error.Code);
        Assert.Contains("frozen", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Closed_HasConflictCode()
    {
        var error = WalletErrors.Closed;
        Assert.Equal("CONFLICT", error.Code);
        Assert.Contains("closed", error.Message, StringComparison.OrdinalIgnoreCase);
    }
}
