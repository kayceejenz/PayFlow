namespace LedgerService.Domain;

public static class LedgerErrors
{
    public static Error InsufficientFunds => Error.Conflict("Insufficient funds to complete the transaction.");
    public static Error AccountNotFound => Error.NotFound("Account not found.");
    public static Error InvalidEntry(string detail) => Error.Validation($"Invalid ledger entry: {detail}");
}
