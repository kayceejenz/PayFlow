namespace WalletService.Features.UpdateWalletStatus;

public record UpdateWalletStatusCommand
{
    public Guid WalletId { get; init; }
    public string Status { get; init; } = string.Empty;
}

public record UpdateWalletStatusResponse
{
    public Guid WalletId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime UpdatedAtUtc { get; init; }
}
