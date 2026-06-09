namespace LedgerService.Features.CreateEntry;

public static class CreateEntryEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/ledger/entries", async (
            CreateEntryCommand command,
            CreateEntryHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(command, ct);
            return result.IsSuccess
                ? Results.Created($"/ledger/entries/{result.Value.EntryId}", result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: result.Error.Code switch
                    {
                        "VALIDATION" => StatusCodes.Status400BadRequest,
                        "NOT_FOUND" => StatusCodes.Status404NotFound,
                        "CONFLICT" => StatusCodes.Status409Conflict,
                        _ => StatusCodes.Status500InternalServerError
                    });
        })
        .WithName("CreateLedgerEntry");

        app.MapPost("/ledger/transactions", async (
            CreateEntryPairCommand command,
            CreateEntryHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandlePairAsync(command, ct);
            return result.IsSuccess
                ? Results.Created($"/ledger/transactions/{result.Value.TransactionId}", result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: result.Error.Code switch
                    {
                        "VALIDATION" => StatusCodes.Status400BadRequest,
                        "CONFLICT" => StatusCodes.Status409Conflict,
                        _ => StatusCodes.Status500InternalServerError
                    });
        })
        .WithName("CreateLedgerTransaction");
    }
}
