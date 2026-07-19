using ApiGateway.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiGateway.Tests;

public class ApiKeyAuthMiddlewareTests
{
    private static async Task<int> RunMiddlewareAsync(
        string? apiKeyHeader, string? configuredKey, string path = "/api/test")
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;

        if (apiKeyHeader != null)
            context.Request.Headers["X-Api-Key"] = apiKeyHeader;

        context.RequestServices = new ServiceCollection()
            .AddSingleton<IConfiguration>(new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ApiKey"] = configuredKey ?? "payflow-demo-key"
                }!)
                .Build())
            .BuildServiceProvider();

        context.Response.Body = new MemoryStream();

        var middleware = new ApiKeyAuthMiddleware(_ => Task.CompletedTask);
        await middleware.InvokeAsync(context);

        return context.Response.StatusCode;
    }

    [Fact]
    public async Task ValidApiKey_Returns200()
    {
        var status = await RunMiddlewareAsync("payflow-demo-key", null);
        Assert.Equal(200, status);
    }

    [Fact]
    public async Task MissingApiKey_Returns401()
    {
        var status = await RunMiddlewareAsync(null, null);
        Assert.Equal(401, status);
    }

    [Fact]
    public async Task InvalidApiKey_Returns401()
    {
        var status = await RunMiddlewareAsync("wrong-key", null);
        Assert.Equal(401, status);
    }

    [Fact]
    public async Task HealthEndpoint_SkipsAuth()
    {
        var status = await RunMiddlewareAsync(null, null, "/health");
        Assert.Equal(200, status);
    }

    [Fact]
    public async Task CustomConfiguredKey_AcceptsCustomKey()
    {
        var status = await RunMiddlewareAsync("my-custom-key", "my-custom-key");
        Assert.Equal(200, status);
    }
}

public class IdempotencyKeyMiddlewareTests
{
    private static async Task<int> RunMiddlewareAsync(string method, string? idempotencyKey)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;

        if (idempotencyKey != null)
            context.Request.Headers["Idempotency-Key"] = idempotencyKey;

        context.RequestServices = new ServiceCollection().BuildServiceProvider();
        context.Response.Body = new MemoryStream();

        var middleware = new IdempotencyKeyMiddleware(_ => Task.CompletedTask);
        await middleware.InvokeAsync(context);

        return context.Response.StatusCode;
    }

    [Fact]
    public async Task PostWithKey_Returns200()
    {
        var status = await RunMiddlewareAsync("POST", "key-123");
        Assert.Equal(200, status);
    }

    [Fact]
    public async Task PostWithoutKey_Returns400()
    {
        var status = await RunMiddlewareAsync("POST", null);
        Assert.Equal(400, status);
    }

    [Fact]
    public async Task GetWithoutKey_Returns200()
    {
        var status = await RunMiddlewareAsync("GET", null);
        Assert.Equal(200, status);
    }

    [Fact]
    public async Task PutWithoutKey_Returns400()
    {
        var status = await RunMiddlewareAsync("PUT", null);
        Assert.Equal(400, status);
    }

    [Fact]
    public async Task DeleteWithoutKey_Returns400()
    {
        var status = await RunMiddlewareAsync("DELETE", null);
        Assert.Equal(400, status);
    }

    [Fact]
    public async Task PatchWithoutKey_Returns400()
    {
        var status = await RunMiddlewareAsync("PATCH", null);
        Assert.Equal(400, status);
    }
}
