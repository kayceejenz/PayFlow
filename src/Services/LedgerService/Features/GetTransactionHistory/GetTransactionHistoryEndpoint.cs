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
        .WithName("GetTransactionHistory")
        .WithTags("Ledger")
        .WithSummary("Get transaction history for an account")
        .WithDescription("Returns all ledger entries for a given account, ordered chronologically. Includes both debit and credit entries.")
        .Produces<GetTransactionHistoryResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
