using System.Net;
using AutoMarket.API.Attributes;
using AutoMarket.API.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutoMarket.Tests.Middleware;

public class IdempotencyMiddlewareTests
{
    private readonly Mock<ILogger<IdempotencyMiddleware>> _loggerMock = new();
    private readonly Mock<IDistributedCache> _cacheMock = new();

    [Fact]
    public async Task InvokeAsync_GetRequest_SinEfecto()
    {
        var called = false;
        var sut = new IdempotencyMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        }, _loggerMock.Object);

        var context = new DefaultHttpContext { Request = { Method = "GET" } };

        await sut.InvokeAsync(context);

        Assert.True(called);
    }

    [Fact]
    public async Task InvokeAsync_PostSinAtributo_SinEfecto()
    {
        var called = false;
        var sut = new IdempotencyMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        }, _loggerMock.Object);

        var context = new DefaultHttpContext
        {
            Request = { Method = "POST" },
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };

        await sut.InvokeAsync(context);

        Assert.True(called);
    }

    [Fact]
    public async Task InvokeAsync_PostConAtributoSinKey_Retorna400()
    {
        var sut = new IdempotencyMiddleware(_ => Task.CompletedTask, _loggerMock.Object);

        var services = new ServiceCollection()
            .AddSingleton(_cacheMock.Object)
            .BuildServiceProvider();

        var context = new DefaultHttpContext
        {
            Request = { Method = "POST" },
            RequestServices = services
        };
        context.SetEndpoint(new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(new object[] { new IdempotentAttribute() }),
            "test"));

        await sut.InvokeAsync(context);

        Assert.Equal((int)HttpStatusCode.BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_PostConKeyYCacheHit_RetornaCacheado()
    {
        var cachedEntry = """{"statusCode":201,"contentType":"application/json","body":"{\"id\":1}"}""";
        var bytes = System.Text.Encoding.UTF8.GetBytes(cachedEntry);
        _cacheMock
            .Setup(c => c.GetAsync("idempotency:POST:key-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        var services = new ServiceCollection()
            .AddSingleton(_cacheMock.Object)
            .BuildServiceProvider();

        var context = new DefaultHttpContext
        {
            Request = { Method = "POST" },
            RequestServices = services
        };
        context.Request.Headers["Idempotency-Key"] = "key-123";
        context.SetEndpoint(new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(new object[] { new IdempotentAttribute() }),
            "test"));

        var sut = new IdempotencyMiddleware(_ => Task.CompletedTask, _loggerMock.Object);

        await sut.InvokeAsync(context);

        Assert.Equal(201, context.Response.StatusCode);
    }
}
