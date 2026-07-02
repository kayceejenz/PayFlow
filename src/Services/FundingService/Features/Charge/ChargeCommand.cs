namespace FundingService.Features.Charge;

public record ChargeCommand
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "GBP";
    public string? Reference { get; init; }
}

public record ChargeResponse
{
    public Guid TransactionId { get; init; }
    public required string Status { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string? FailureReason { get; init; }
}
