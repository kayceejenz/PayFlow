using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace WalletService.Infrastructure;

public interface IFundingServiceClient
{
    Task<FundingChargeResponse> ChargeAsync(string idempotencyKey, FundingChargeRequest request, CancellationToken ct);
}

public class FundingServiceClient : IFundingServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FundingServiceClient> _logger;

    public FundingServiceClient(HttpClient httpClient, ILogger<FundingServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<FundingChargeResponse> ChargeAsync(
        string idempotencyKey, FundingChargeRequest request, CancellationToken ct)
    {
        _httpClient.DefaultRequestHeaders.Remove("Idempotency-Key");
        _httpClient.DefaultRequestHeaders.Add("Idempotency-Key", idempotencyKey);

        var response = await _httpClient.PostAsJsonAsync("/funding/charge", request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<FundingChargeResponse>(cancellationToken: ct);
        return result ?? throw new InvalidOperationException("Funding service returned null response.");
    }
}

public record FundingChargeRequest
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "GBP";
    public string? Reference { get; init; }
}

public record FundingChargeResponse
{
    public Guid TransactionId { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string? FailureReason { get; init; }
}
