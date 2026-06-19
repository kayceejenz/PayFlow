using System.Diagnostics;
using WalletService.Domain;
using WalletService.Infrastructure;
using PayFlow.Shared.Observability;

namespace WalletService.Features.UpdateWalletStatus;

public class UpdateWalletStatusHandler
{
    private readonly IWalletRepository _repository;
    private readonly ILogger<UpdateWalletStatusHandler> _logger;

    public UpdateWalletStatusHandler(IWalletRepository repository, ILogger<UpdateWalletStatusHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<UpdateWalletStatusResponse>> HandleAsync(UpdateWalletStatusCommand command, CancellationToken ct)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("UpdateWalletStatus");
        activity?.SetTag("wallet.id", command.WalletId);

        if (!Enum.TryParse<WalletStatus>(command.Status, true, out var newStatus))
            return Result.Failure<UpdateWalletStatusResponse>(Error.Validation($"Invalid wallet status: {command.Status}"));

        var wallet = await _repository.GetByIdAsync(command.WalletId, ct);

        if (wallet == null)
            return Result.Failure<UpdateWalletStatusResponse>(WalletErrors.NotFound);

        if (wallet.Status == WalletStatus.Closed)
            return Result.Failure<UpdateWalletStatusResponse>(WalletErrors.AlreadyClosed);

        if (wallet.Status == newStatus)
        {
            if (newStatus == WalletStatus.Frozen)
                return Result.Failure<UpdateWalletStatusResponse>(WalletErrors.AlreadyFrozen);
            return Result.Success(new UpdateWalletStatusResponse
            {
                WalletId = wallet.Id,
                Status = wallet.Status.ToString(),
                UpdatedAtUtc = wallet.UpdatedAtUtc
            });
        }

        wallet.Status = newStatus;
        wallet.UpdatedAtUtc = DateTime.UtcNow;

        await _repository.UpdateAsync(wallet, ct);

        activity?.SetTag("new.status", wallet.Status.ToString());

        _logger.LogInformation(
            "Updated wallet {WalletId} status to {Status}",
            wallet.Id, wallet.Status);

        return Result.Success(new UpdateWalletStatusResponse
        {
            WalletId = wallet.Id,
            Status = wallet.Status.ToString(),
            UpdatedAtUtc = wallet.UpdatedAtUtc
        });
    }
}
