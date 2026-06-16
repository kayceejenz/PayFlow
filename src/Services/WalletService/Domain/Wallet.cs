namespace WalletService.Domain;

public class Wallet
{
    public Guid Id { get; init; }
    public Guid AccountId { get; init; }
    public WalletStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime UpdatedAtUtc { get; set; }
}
