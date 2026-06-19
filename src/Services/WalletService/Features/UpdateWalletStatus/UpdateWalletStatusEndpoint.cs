namespace WalletService.Features.UpdateWalletStatus;

public static class UpdateWalletStatusEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPatch("/wallets/{walletId}/status", async (
            Guid walletId,
            UpdateWalletStatusCommand command,
            UpdateWalletStatusHandler handler,
            CancellationToken ct) =>
        {
            command = command with { WalletId = walletId };
            var result = await handler.HandleAsync(command, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: result.Error.Code switch
                    {
                        "NOT_FOUND" => StatusCodes.Status404NotFound,
                        "CONFLICT" => StatusCodes.Status409Conflict,
                        "VALIDATION" => StatusCodes.Status400BadRequest,
                        _ => StatusCodes.Status500InternalServerError
                    });
        })
        .WithName("UpdateWalletStatus")
        .WithTags("Wallets")
        .WithSummary("Update wallet status (freeze/unfreeze/close)")
        .WithDescription("Updates the status of a wallet. Allowed transitions: Active -> Frozen, Frozen -> Active, Active -> Closed.")
        .Produces<UpdateWalletStatusResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
