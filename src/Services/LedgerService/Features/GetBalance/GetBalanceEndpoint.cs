namespace LedgerService.Features.GetBalance;

public static class GetBalanceEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/ledger/accounts/{accountId:guid}/balance", async (
            Guid accountId,
            GetBalanceHandler handler,
            CancellationToken ct) =>
        {
            var query = new GetBalanceQuery(accountId);
            var result = await handler.HandleAsync(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { error = result.Error.Message });
        })
        .WithName("GetAccountBalance")
        .WithTags("Ledger")
        .WithSummary("Get account balance")
        .WithDescription("Returns the current balance for a given account. Balance is calculated as total credits minus total debits.")
        .Produces<GetBalanceResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
