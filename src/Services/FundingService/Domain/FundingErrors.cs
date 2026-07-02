namespace FundingService.Domain;

public static class FundingErrors
{
    public static Error ChargeFailed(string reason) => Error.Conflict($"Charge failed: {reason}");
    public static Error InvalidAmount => Error.Validation("Amount must be positive.");
    public static Error ServiceUnavailable => Error.Unexpected("Funding service temporarily unavailable.");
}
