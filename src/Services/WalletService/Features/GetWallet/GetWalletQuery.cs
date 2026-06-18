namespace WalletService.Features.GetWallet;

public record GetWalletQuery(Guid WalletId);

public record GetWalletResponse
{
    public Guid WalletId { get; init; }
    public Guid AccountId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
}
