namespace PaymentService.Features.GetPayment;

public record GetPaymentQuery
{
    public Guid PaymentId { get; init; }
}

public record GetPaymentResponse
{
    public Guid PaymentId { get; init; }
    public Guid PayerAccountId { get; init; }
    public Guid MerchantAccountId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? Reference { get; init; }
    public string? FailureReason { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
}
