using PayFlow.Shared.Primitives;

namespace LedgerService.Domain;

public record TransactionId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static TransactionId New() => new(Guid.NewGuid());
}
