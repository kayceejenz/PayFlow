namespace PaymentService.Domain;

public record PaymentId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static PaymentId New() => new(Guid.NewGuid());
}
