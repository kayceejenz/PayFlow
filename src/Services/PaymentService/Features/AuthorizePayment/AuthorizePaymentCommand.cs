namespace PaymentService.Features.AuthorizePayment;

public record AuthorizePaymentCommand
{
    public Guid PayerAccountId { get; init; }
    public Guid MerchantAccountId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "GBP";
    public string? Reference { get; init; }
}

public record AuthorizePaymentResponse
{
    public Guid PaymentId { get; init; }
    public string Status { get; init; } = "pending_authorization";
}
