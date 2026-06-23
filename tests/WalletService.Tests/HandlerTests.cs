using Microsoft.Extensions.Logging;
using NSubstitute;
using WalletService.Domain;
using WalletService.Features.CreateWallet;
using WalletService.Features.GetWallet;
using WalletService.Features.UpdateWalletStatus;
using WalletService.Features.TopUp;
using WalletService.Features.Transfer;
using WalletService.Infrastructure;

namespace WalletService.Tests;

public class CreateWalletHandlerTests
{
    private readonly IWalletRepository _repository;
    private readonly CreateWalletHandler _handler;

    public CreateWalletHandlerTests()
    {
        _repository = Substitute.For<IWalletRepository>();
        var logger = Substitute.For<ILogger<CreateWalletHandler>>();
        _handler = new CreateWalletHandler(_repository, logger);
    }

    [Fact]
    public async Task HandleAsync_CreatesWallet_ReturnsSuccessWithWalletId()
    {
        var command = new CreateWalletCommand();

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.WalletId);
        Assert.NotEqual(Guid.Empty, result.Value.AccountId);
        Assert.Equal("Active", result.Value.Status);
    }

    [Fact]
    public async Task HandleAsync_PersistsWalletToRepository()
    {
        var command = new CreateWalletCommand();
        Wallet? capturedWallet = null;
        await _repository.AddAsync(Arg.Do<Wallet>(w => capturedWallet = w), Arg.Any<CancellationToken>());

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedWallet);
        Assert.Equal(result.Value.WalletId, capturedWallet.Id);
        Assert.Equal(result.Value.AccountId, capturedWallet.AccountId);
        Assert.Equal(WalletStatus.Active, capturedWallet.Status);
    }
}

public class GetWalletHandlerTests
{
    private readonly IWalletRepository _repository;
    private readonly GetWalletHandler _handler;

    public GetWalletHandlerTests()
    {
        _repository = Substitute.For<IWalletRepository>();
        _handler = new GetWalletHandler(_repository);
    }

    [Fact]
    public async Task HandleAsync_WalletExists_ReturnsWallet()
    {
        var walletId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var wallet = new Wallet
        {
            Id = walletId,
            AccountId = accountId,
            Status = WalletStatus.Active,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        _repository.GetByIdAsync(walletId, Arg.Any<CancellationToken>()).Returns(wallet);

        var result = await _handler.HandleAsync(new GetWalletQuery(walletId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(walletId, result.Value.WalletId);
        Assert.Equal(accountId, result.Value.AccountId);
        Assert.Equal("Active", result.Value.Status);
    }

    [Fact]
    public async Task HandleAsync_WalletNotFound_ReturnsFailure()
    {
        var walletId = Guid.NewGuid();
        _repository.GetByIdAsync(walletId, Arg.Any<CancellationToken>()).Returns((Wallet?)null);

        var result = await _handler.HandleAsync(new GetWalletQuery(walletId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND", result.Error.Code);
    }
}

public class UpdateWalletStatusHandlerTests
{
    private readonly IWalletRepository _repository;
    private readonly UpdateWalletStatusHandler _handler;

    public UpdateWalletStatusHandlerTests()
    {
        _repository = Substitute.For<IWalletRepository>();
        var logger = Substitute.For<ILogger<UpdateWalletStatusHandler>>();
        _handler = new UpdateWalletStatusHandler(_repository, logger);
    }

    [Fact]
    public async Task HandleAsync_FreezeActiveWallet_ReturnsFrozen()
    {
        var walletId = Guid.NewGuid();
        var wallet = new Wallet
        {
            Id = walletId,
            AccountId = Guid.NewGuid(),
            Status = WalletStatus.Active,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _repository.GetByIdAsync(walletId, Arg.Any<CancellationToken>()).Returns(wallet);

        var result = await _handler.HandleAsync(
            new UpdateWalletStatusCommand { WalletId = walletId, Status = "Frozen" },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Frozen", result.Value.Status);
        Assert.Equal(WalletStatus.Frozen, wallet.Status);
    }

    [Fact]
    public async Task HandleAsync_FreezeAlreadyFrozenWallet_ReturnsConflict()
    {
        var walletId = Guid.NewGuid();
        var wallet = new Wallet
        {
            Id = walletId,
            AccountId = Guid.NewGuid(),
            Status = WalletStatus.Frozen,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _repository.GetByIdAsync(walletId, Arg.Any<CancellationToken>()).Returns(wallet);

        var result = await _handler.HandleAsync(
            new UpdateWalletStatusCommand { WalletId = walletId, Status = "Frozen" },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_CloseClosedWallet_ReturnsConflict()
    {
        var walletId = Guid.NewGuid();
        var wallet = new Wallet
        {
            Id = walletId,
            AccountId = Guid.NewGuid(),
            Status = WalletStatus.Closed,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _repository.GetByIdAsync(walletId, Arg.Any<CancellationToken>()).Returns(wallet);

        var result = await _handler.HandleAsync(
            new UpdateWalletStatusCommand { WalletId = walletId, Status = "Active" },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_InvalidStatus_ReturnsValidationError()
    {
        var result = await _handler.HandleAsync(
            new UpdateWalletStatusCommand { WalletId = Guid.NewGuid(), Status = "Invalid" },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_WalletNotFound_ReturnsNotFound()
    {
        var walletId = Guid.NewGuid();
        _repository.GetByIdAsync(walletId, Arg.Any<CancellationToken>()).Returns((Wallet?)null);

        var result = await _handler.HandleAsync(
            new UpdateWalletStatusCommand { WalletId = walletId, Status = "Frozen" },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND", result.Error.Code);
    }
}

public class TopUpHandlerTests
{
    private readonly IWalletRepository _repository;
    private readonly TopUpHandler _handler;

    public TopUpHandlerTests()
    {
        _repository = Substitute.For<IWalletRepository>();
        var logger = Substitute.For<ILogger<TopUpHandler>>();
        _handler = new TopUpHandler(_repository, logger);
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsAccepted()
    {
        var walletId = Guid.NewGuid();
        var wallet = new Wallet
        {
            Id = walletId,
            AccountId = Guid.NewGuid(),
            Status = WalletStatus.Active,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _repository.GetByIdAsync(walletId, Arg.Any<CancellationToken>()).Returns(wallet);

        var result = await _handler.HandleAsync(
            new TopUpCommand { WalletId = walletId, Amount = 100, Currency = "GBP" },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.CorrelationId);
        Assert.Equal("pending", result.Value.Status);
    }

    [Fact]
    public async Task HandleAsync_ZeroAmount_ReturnsValidationError()
    {
        var result = await _handler.HandleAsync(
            new TopUpCommand { WalletId = Guid.NewGuid(), Amount = 0 },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_NegativeAmount_ReturnsValidationError()
    {
        var result = await _handler.HandleAsync(
            new TopUpCommand { WalletId = Guid.NewGuid(), Amount = -50 },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_FrozenWallet_ReturnsConflict()
    {
        var walletId = Guid.NewGuid();
        var wallet = new Wallet
        {
            Id = walletId,
            AccountId = Guid.NewGuid(),
            Status = WalletStatus.Frozen,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _repository.GetByIdAsync(walletId, Arg.Any<CancellationToken>()).Returns(wallet);

        var result = await _handler.HandleAsync(
            new TopUpCommand { WalletId = walletId, Amount = 100 },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_ClosedWallet_ReturnsConflict()
    {
        var walletId = Guid.NewGuid();
        var wallet = new Wallet
        {
            Id = walletId,
            AccountId = Guid.NewGuid(),
            Status = WalletStatus.Closed,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _repository.GetByIdAsync(walletId, Arg.Any<CancellationToken>()).Returns(wallet);

        var result = await _handler.HandleAsync(
            new TopUpCommand { WalletId = walletId, Amount = 100 },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_WalletNotFound_ReturnsNotFound()
    {
        var walletId = Guid.NewGuid();
        _repository.GetByIdAsync(walletId, Arg.Any<CancellationToken>()).Returns((Wallet?)null);

        var result = await _handler.HandleAsync(
            new TopUpCommand { WalletId = walletId, Amount = 100 },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_SavesOutboxMessage()
    {
        var walletId = Guid.NewGuid();
        var wallet = new Wallet
        {
            Id = walletId,
            AccountId = Guid.NewGuid(),
            Status = WalletStatus.Active,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _repository.GetByIdAsync(walletId, Arg.Any<CancellationToken>()).Returns(wallet);

        OutboxMessage? capturedMessage = null;
        await _repository.SaveOutboxMessageAsync(Arg.Do<OutboxMessage>(m => capturedMessage = m), Arg.Any<CancellationToken>());

        var result = await _handler.HandleAsync(
            new TopUpCommand { WalletId = walletId, Amount = 100, Currency = "GBP", Reference = "test-topup" },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedMessage);
        Assert.Equal(result.Value.CorrelationId, capturedMessage.CorrelationId);
        Assert.Contains("CreateLedgerEntryCommand", capturedMessage.Type);
        Assert.NotEmpty(capturedMessage.Payload);
    }
}

public class TransferHandlerTests
{
    private readonly IWalletRepository _repository;
    private readonly TransferHandler _handler;

    public TransferHandlerTests()
    {
        _repository = Substitute.For<IWalletRepository>();
        var logger = Substitute.For<ILogger<TransferHandler>>();
        _handler = new TransferHandler(_repository, logger);
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsAccepted()
    {
        var sourceId = Guid.NewGuid();
        var destId = Guid.NewGuid();
        var sourceWallet = new Wallet
        {
            Id = sourceId,
            AccountId = Guid.NewGuid(),
            Status = WalletStatus.Active,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        var destWallet = new Wallet
        {
            Id = destId,
            AccountId = Guid.NewGuid(),
            Status = WalletStatus.Active,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _repository.GetByIdAsync(sourceId, Arg.Any<CancellationToken>()).Returns(sourceWallet);
        _repository.GetByIdAsync(destId, Arg.Any<CancellationToken>()).Returns(destWallet);

        var result = await _handler.HandleAsync(
            new TransferCommand { WalletId = sourceId, DestinationWalletId = destId, Amount = 50, Currency = "GBP" },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.CorrelationId);
        Assert.Equal("pending", result.Value.Status);
    }

    [Fact]
    public async Task HandleAsync_ZeroAmount_ReturnsValidationError()
    {
        var result = await _handler.HandleAsync(
            new TransferCommand { WalletId = Guid.NewGuid(), DestinationWalletId = Guid.NewGuid(), Amount = 0 },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_SameWallet_ReturnsValidationError()
    {
        var id = Guid.NewGuid();
        var result = await _handler.HandleAsync(
            new TransferCommand { WalletId = id, DestinationWalletId = id, Amount = 50 },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_SourceWalletNotFound_ReturnsNotFound()
    {
        var sourceId = Guid.NewGuid();
        _repository.GetByIdAsync(sourceId, Arg.Any<CancellationToken>()).Returns((Wallet?)null);

        var result = await _handler.HandleAsync(
            new TransferCommand { WalletId = sourceId, DestinationWalletId = Guid.NewGuid(), Amount = 50 },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_DestinationWalletNotFound_ReturnsNotFound()
    {
        var sourceId = Guid.NewGuid();
        var destId = Guid.NewGuid();
        var sourceWallet = new Wallet
        {
            Id = sourceId,
            AccountId = Guid.NewGuid(),
            Status = WalletStatus.Active,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _repository.GetByIdAsync(sourceId, Arg.Any<CancellationToken>()).Returns(sourceWallet);
        _repository.GetByIdAsync(destId, Arg.Any<CancellationToken>()).Returns((Wallet?)null);

        var result = await _handler.HandleAsync(
            new TransferCommand { WalletId = sourceId, DestinationWalletId = destId, Amount = 50 },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_SourceWalletFrozen_ReturnsConflict()
    {
        var sourceId = Guid.NewGuid();
        var sourceWallet = new Wallet
        {
            Id = sourceId,
            AccountId = Guid.NewGuid(),
            Status = WalletStatus.Frozen,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _repository.GetByIdAsync(sourceId, Arg.Any<CancellationToken>()).Returns(sourceWallet);

        var result = await _handler.HandleAsync(
            new TransferCommand { WalletId = sourceId, DestinationWalletId = Guid.NewGuid(), Amount = 50 },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_DestinationWalletNotActive_ReturnsConflict()
    {
        var sourceId = Guid.NewGuid();
        var destId = Guid.NewGuid();
        var sourceWallet = new Wallet
        {
            Id = sourceId,
            AccountId = Guid.NewGuid(),
            Status = WalletStatus.Active,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        var destWallet = new Wallet
        {
            Id = destId,
            AccountId = Guid.NewGuid(),
            Status = WalletStatus.Frozen,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _repository.GetByIdAsync(sourceId, Arg.Any<CancellationToken>()).Returns(sourceWallet);
        _repository.GetByIdAsync(destId, Arg.Any<CancellationToken>()).Returns(destWallet);

        var result = await _handler.HandleAsync(
            new TransferCommand { WalletId = sourceId, DestinationWalletId = destId, Amount = 50 },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT", result.Error.Code);
    }
}
