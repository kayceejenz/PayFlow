using FundingService.Domain;

namespace FundingService.Tests;

public class FundingErrorsTests
{
    [Fact]
    public void ChargeFailed_HasConflictCode()
    {
        var error = FundingErrors.ChargeFailed("test reason");
        Assert.Equal("CONFLICT", error.Code);
        Assert.Contains("test reason", error.Message);
    }

    [Fact]
    public void InvalidAmount_HasValidationCode()
    {
        var error = FundingErrors.InvalidAmount;
        Assert.Equal("VALIDATION", error.Code);
    }

    [Fact]
    public void ServiceUnavailable_HasUnexpectedCode()
    {
        var error = FundingErrors.ServiceUnavailable;
        Assert.Equal("UNEXPECTED", error.Code);
    }
}
