using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AutoMarket.API.HealthChecks;

public class DatabasePoolHealthCheck : IHealthCheck
{
    private readonly IDbContextFactory<AutoMarket.Infrastructure.Data.ApplicationDbContext> _factory;

    public DatabasePoolHealthCheck(
        IDbContextFactory<AutoMarket.Infrastructure.Data.ApplicationDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var dbContext = await _factory.CreateDbContextAsync(cancellationToken);
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy("No se pudo conectar a la base de datos.");
            }

            return HealthCheckResult.Healthy("Pool de conexiones disponible.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Excepción al verificar el pool de conexiones.", ex);
        }
    }
}
