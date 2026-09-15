using AutoMarket.API.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AutoMarket.API.Extensions;

public static class HealthCheckExtensions
{
    public static void AddApplicationHealthChecks(
        this IServiceCollection services,
        string connectionString,
        string redisHost,
        string redisPort,
        string redisPassword,
        string? paypalUrlBase)
    {
        services
            .AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("API está funcionando"))
            .AddNpgSql(connectionString, name: "postgres", tags: ["database", "ready"])
            .AddCheck<DatabasePoolHealthCheck>("db_pool", tags: ["database", "pool", "ready"])
            .AddRedis(
                $"{redisHost}:{redisPort},password={redisPassword},abortConnect=false",
                name: "redis",
                failureStatus: HealthStatus.Degraded,
                tags: ["cache", "ready"]);

        if (!string.IsNullOrWhiteSpace(paypalUrlBase))
        {
            services
                .AddHealthChecks()
                .AddUrlGroup(
                    new Uri(paypalUrlBase),
                    name: "paypal",
                    failureStatus: HealthStatus.Degraded,
                    tags: ["payments", "external"]);
        }
    }
}