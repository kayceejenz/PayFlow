namespace PaymentService.Domain;

public static class PaymentErrors
{
    public static Error NotFound => Error.NotFound("Payment not found.");
    public static Error NotAuthorized => Error.Conflict("Payment is not in Authorized state.");
    public static Error AlreadyCaptured => Error.Conflict("Payment has already been captured.");
    public static Error AlreadyReleased => Error.Conflict("Payment has already been released.");
    public static Error InvalidState(string state) => Error.Conflict($"Payment is in {state} state and cannot perform this operation.");
    public static Error AmountExceedsHold => Error.Validation("Capture amount exceeds the authorized hold amount.");
}
