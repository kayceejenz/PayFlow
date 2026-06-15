namespace WalletService.Domain;

public record WalletId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static WalletId New() => new(Guid.NewGuid());
}
