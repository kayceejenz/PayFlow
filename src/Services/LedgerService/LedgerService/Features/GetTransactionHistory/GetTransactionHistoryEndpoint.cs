namespace LedgerService.Features.GetTransactionHistory;

public static class GetTransactionHistoryEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/ledger/accounts/{accountId:guid}/transactions", async (
            Guid accountId,
            GetTransactionHistoryHandler handler,
            CancellationToken ct) =>
        {
            var query = new GetTransactionHistoryQuery(accountId);
            var result = await handler.HandleAsync(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { error = result.Error.Message });
        })
        .WithName("GetTransactionHistory");
    }
}
