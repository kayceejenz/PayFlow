using System.Diagnostics;
using WalletService.Domain;
using WalletService.Infrastructure;
using PayFlow.Shared.Observability;

namespace WalletService.Features.GetWallet;

public class GetWalletHandler
{
    private readonly IWalletRepository _repository;

    public GetWalletHandler(IWalletRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<GetWalletResponse>> HandleAsync(GetWalletQuery query, CancellationToken ct)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("GetWallet");
        activity?.SetTag("wallet.id", query.WalletId);

        var wallet = await _repository.GetByIdAsync(query.WalletId, ct);

        if (wallet == null)
            return Result.Failure<GetWalletResponse>(WalletErrors.NotFound);

        activity?.SetTag("account.id", wallet.AccountId);
        activity?.SetTag("wallet.status", wallet.Status.ToString());

        return Result.Success(new GetWalletResponse
        {
            WalletId = wallet.Id,
            AccountId = wallet.AccountId,
            Status = wallet.Status.ToString(),
            CreatedAtUtc = wallet.CreatedAtUtc,
            UpdatedAtUtc = wallet.UpdatedAtUtc
        });
    }
}
