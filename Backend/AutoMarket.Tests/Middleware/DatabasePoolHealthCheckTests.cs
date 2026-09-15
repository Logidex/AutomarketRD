using AutoMarket.API.HealthChecks;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;

namespace AutoMarket.Tests.Middleware;

public class DatabasePoolHealthCheckTests
{
    private readonly Mock<IDbContextFactory<ApplicationDbContext>> _factoryMock = new();

    [Fact]
    public async Task CheckHealthAsync_ConexionExitosa_RetornaHealthy()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"test_pool_{Guid.NewGuid()}")
            .Options;

        var context = new ApplicationDbContext(options);
        _factoryMock
            .Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(context);

        var sut = new DatabasePoolHealthCheck(_factoryMock.Object);
        var healthContext = new HealthCheckContext();

        var result = await sut.CheckHealthAsync(healthContext);

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_ConexionFalla_RetornaUnhealthy()
    {
        _factoryMock
            .Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Connection failed"));

        var sut = new DatabasePoolHealthCheck(_factoryMock.Object);
        var healthContext = new HealthCheckContext();

        var result = await sut.CheckHealthAsync(healthContext);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }
}
