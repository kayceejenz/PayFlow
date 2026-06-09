namespace LedgerService.Features.GetBalance;

public record GetBalanceQuery(Guid AccountId);

public record GetBalanceResponse(Guid AccountId, decimal Balance, string Currency);
