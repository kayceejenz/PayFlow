namespace ApiGateway.Middleware;

public class IdempotencyKeyMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly string[] MutatingMethods = ["POST", "PUT", "PATCH", "DELETE"];

    public IdempotencyKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (MutatingMethods.Contains(context.Request.Method, StringComparer.OrdinalIgnoreCase)
            && !context.Request.Headers.ContainsKey("Idempotency-Key"))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = "Idempotency-Key header is required for mutating requests" });
            return;
        }

        await _next(context);
    }
}
