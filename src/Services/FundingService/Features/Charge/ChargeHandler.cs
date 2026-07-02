using System.Diagnostics;
using FundingService.Domain;
using FundingService.Infrastructure;
using PayFlow.Shared.Observability;

namespace FundingService.Features.Charge;

public class ChargeHandler
{
    private readonly IIdempotencyStore _idempotencyStore;
    private readonly ILogger<ChargeHandler> _logger;
    private readonly Random _random = new();

    public ChargeHandler(IIdempotencyStore idempotencyStore, ILogger<ChargeHandler> logger)
    {
        _idempotencyStore = idempotencyStore;
        _logger = logger;
    }

    public async Task<Result<ChargeResponse>> HandleAsync(
        string idempotencyKey, ChargeCommand command, CancellationToken ct)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("FundingCharge");
        activity?.SetTag("amount", command.Amount);
        activity?.SetTag("currency", command.Currency);
        activity?.SetTag("idempotency.key", idempotencyKey);

        if (command.Amount <= 0)
            return Result.Failure<ChargeResponse>(FundingErrors.InvalidAmount);

        var cached = await _idempotencyStore.GetAsync(idempotencyKey, ct);
        if (cached != null)
        {
            activity?.SetTag("cached", true);
            _logger.LogInformation("Returning cached charge result for key {Key}", idempotencyKey);
            return Result.Success(cached);
        }

        var failureRate = FailureRate;
        var minDelay = MinDelayMs;
        var maxDelay = MaxDelayMs;

        var delay = _random.Next(minDelay, maxDelay + 1);
        await Task.Delay(delay, ct);

        var isFailure = _random.NextDouble() < failureRate;

        ChargeResponse response;
        if (isFailure)
        {
            response = new ChargeResponse
            {
                TransactionId = Guid.NewGuid(),
                Status = "failed",
                Amount = command.Amount,
                Currency = command.Currency,
                FailureReason = "Insufficient funds"
            };

            activity?.SetTag("charge.status", "failed");
            _logger.LogWarning("Charge failed: {Amount} {Currency} (key {Key})",
                command.Amount, command.Currency, idempotencyKey);
        }
        else
        {
            response = new ChargeResponse
            {
                TransactionId = Guid.NewGuid(),
                Status = "succeeded",
                Amount = command.Amount,
                Currency = command.Currency
            };

            activity?.SetTag("charge.status", "succeeded");
            _logger.LogInformation("Charge succeeded: {Amount} {Currency} (key {Key}, txn {TxnId})",
                command.Amount, command.Currency, idempotencyKey, response.TransactionId);
        }

        await _idempotencyStore.SetAsync(idempotencyKey, response, ct);

        return Result.Success(response);
    }

    private static double FailureRate =>
        double.TryParse(Environment.GetEnvironmentVariable("Funding__FailureRate"), out var f) ? f : 0.2;

    private static int MinDelayMs =>
        int.TryParse(Environment.GetEnvironmentVariable("Funding__MinDelayMs"), out var min) ? min : 100;

    private static int MaxDelayMs =>
        int.TryParse(Environment.GetEnvironmentVariable("Funding__MaxDelayMs"), out var max) ? max : 500;
}
