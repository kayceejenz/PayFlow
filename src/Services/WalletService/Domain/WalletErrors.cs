namespace WalletService.Domain;

public static class WalletErrors
{
    public static Error NotFound => Error.NotFound("Wallet not found.");
    public static Error AlreadyFrozen => Error.Conflict("Wallet is already frozen.");
    public static Error AlreadyClosed => Error.Conflict("Wallet is already closed.");
    public static Error Frozen => Error.Conflict("Wallet is frozen. Operation not allowed.");
    public static Error Closed => Error.Conflict("Wallet is closed. Operation not allowed.");
}
