using AutoMarket.API.Middleware;
using Microsoft.AspNetCore.Http;

namespace AutoMarket.Tests.Middleware;

public class CorrelationIdMiddlewareTests
{
    private readonly CorrelationIdMiddleware _sut;

    public CorrelationIdMiddlewareTests()
    {
        _sut = new CorrelationIdMiddleware(_ => Task.CompletedTask);
    }

    [Fact]
    public async Task InvokeAsync_ConSinHeader_GeneraCorrelationId()
    {
        var context = new DefaultHttpContext();

        await _sut.InvokeAsync(context);

        Assert.NotNull(context.Items["CorrelationId"]);
        Assert.NotEmpty(context.Items["CorrelationId"]!.ToString()!);
    }

    [Fact]
    public async Task InvokeAsync_ConHeaderExistente_UsaElMismo()
    {
        var expected = "test-id-123";
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-Id"] = expected;

        await _sut.InvokeAsync(context);

        Assert.Equal(expected, context.Items["CorrelationId"]);
    }

    [Fact]
    public async Task InvokeAsync_EscribeCorrelationIdEnResponse()
    {
        var context = new DefaultHttpContext();

        await _sut.InvokeAsync(context);

        Assert.True(context.Response.Headers.ContainsKey("X-Correlation-Id"));
        Assert.NotEmpty(context.Response.Headers["X-Correlation-Id"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_LlamaAlSiguienteMiddleware()
    {
        var called = false;
        var sut = new CorrelationIdMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });
        var context = new DefaultHttpContext();

        await sut.InvokeAsync(context);

        Assert.True(called);
    }
}
