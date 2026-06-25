namespace PaymentService.Domain;

public class Payment
{
    public Guid Id { get; init; }
    public Guid PayerAccountId { get; init; }
    public Guid MerchantAccountId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "GBP";
    public PaymentStatus Status { get; set; }
    public string? Reference { get; init; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime UpdatedAtUtc { get; set; }
}
