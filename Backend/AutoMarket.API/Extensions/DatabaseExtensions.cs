using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.API.Extensions;

public static class DatabaseExtensions
{
    public static string AddApplicationDatabase(this IServiceCollection services, WebApplicationBuilder builder)
    {
        var connectionString =
            builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection");

        // DbContextFactory con pooling: registra ApplicationDbContext como
        // scoped y IDbContextFactory<ApplicationDbContext> como singleton,
        // sin el conflicto de validación de DI que rompía el arranque.
        services.AddPooledDbContextFactory<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.CommandTimeout(30);
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            }));

        return connectionString;
    }

    public static RedisConnectionInfo AddRedisCache(this IServiceCollection services, WebApplicationBuilder builder)
    {
        var redisHost = builder.Configuration["Redis:Host"] ?? "redis";
        var redisPort = builder.Configuration["Redis:Port"] ?? "6379";
        var redisPassword = builder.Configuration["Redis:Password"] ?? "";
        var redisInstanceName = builder.Configuration["Redis:InstanceName"] ?? "automarket_";

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = $"{redisHost}:{redisPort},password={redisPassword},abortConnect=false,connectRetry=3,connectTimeout=5000,syncTimeout=5000";
            options.InstanceName = redisInstanceName;
        });

        return new RedisConnectionInfo(redisHost, redisPort, redisPassword);
    }
}

public sealed record RedisConnectionInfo(string Host, string Port, string Password);