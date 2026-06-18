namespace WalletService.Features.CreateWallet;

public record CreateWalletCommand
{
    public Guid? IdempotencyKey { get; init; }
}

public record CreateWalletResponse
{
    public Guid WalletId { get; init; }
    public Guid AccountId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
}
