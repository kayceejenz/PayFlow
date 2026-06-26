namespace PaymentService.Features.CapturePayment;

public record CapturePaymentCommand
{
    public Guid PaymentId { get; init; }
}

public record CapturePaymentResponse
{
    public Guid PaymentId { get; init; }
    public string Status { get; init; } = "processing_capture";
}
