namespace WalletService.Features.Transfer;

public record TransferCommand
{
    public Guid WalletId { get; init; }
    public Guid DestinationWalletId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "GBP";
    public string? Reference { get; init; }
}

public record TransferResponse
{
    public Guid CorrelationId { get; init; }
    public string Status { get; init; } = "pending";
}
