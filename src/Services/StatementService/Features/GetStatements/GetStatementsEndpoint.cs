namespace StatementService.Features.GetStatements;

public static class GetStatementsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/statements/{walletId:guid}", async (
            Guid walletId,
            int page,
            int pageSize,
            GetStatementsHandler handler,
            CancellationToken ct) =>
        {
            var query = new GetStatementsQuery(walletId, page, pageSize);
            var response = await handler.HandleAsync(query, ct);
            return Results.Ok(response);
        })
        .WithName("GetStatements")
        .WithTags("Statements")
        .WithSummary("Get paginated transaction history for a wallet")
        .Produces<GetStatementsResponse>(StatusCodes.Status200OK);
    }
}
