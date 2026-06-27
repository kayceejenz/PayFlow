namespace PaymentService.Features.ReleasePayment;

public record ReleasePaymentCommand
{
    public Guid PaymentId { get; init; }
}

public record ReleasePaymentResponse
{
    public Guid PaymentId { get; init; }
    public string Status { get; init; } = "processing_release";
}
