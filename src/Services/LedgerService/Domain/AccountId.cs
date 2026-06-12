using PayFlow.Shared.Primitives;

namespace LedgerService.Domain;

public record AccountId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static AccountId New() => new(Guid.NewGuid());
}
