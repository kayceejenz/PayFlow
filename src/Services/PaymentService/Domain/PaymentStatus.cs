namespace PaymentService.Domain;

public enum PaymentStatus
{
    PendingAuthorization,
    Authorized,
    ProcessingCapture,
    Captured,
    ProcessingRelease,
    Released,
    Failed
}
