namespace WalletService.Features.TopUp;

public record TopUpCommand
{
    public Guid WalletId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "GBP";
    public string? Reference { get; init; }
    public string? IdempotencyKey { get; init; }
}

public record TopUpResponse
{
    public Guid CorrelationId { get; init; }
    public string Status { get; init; } = "pending";
}
