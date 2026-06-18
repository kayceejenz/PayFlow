using System.Diagnostics;
using WalletService.Domain;
using WalletService.Infrastructure;
using PayFlow.Shared.Observability;

namespace WalletService.Features.CreateWallet;

public class CreateWalletHandler
{
    private readonly IWalletRepository _repository;
    private readonly ILogger<CreateWalletHandler> _logger;

    public CreateWalletHandler(IWalletRepository repository, ILogger<CreateWalletHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<CreateWalletResponse>> HandleAsync(CreateWalletCommand command, CancellationToken ct)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("CreateWallet");

        var walletId = WalletId.New();
        var accountId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var wallet = new Wallet
        {
            Id = walletId.Value,
            AccountId = accountId,
            Status = WalletStatus.Active,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        await _repository.AddAsync(wallet, ct);

        activity?.SetTag("wallet.id", wallet.Id);
        activity?.SetTag("account.id", wallet.AccountId);

        _logger.LogInformation(
            "Created wallet {WalletId} with account {AccountId}",
            wallet.Id, wallet.AccountId);

        return Result.Success(new CreateWalletResponse
        {
            WalletId = wallet.Id,
            AccountId = wallet.AccountId,
            Status = wallet.Status.ToString(),
            CreatedAtUtc = wallet.CreatedAtUtc
        });
    }
}
