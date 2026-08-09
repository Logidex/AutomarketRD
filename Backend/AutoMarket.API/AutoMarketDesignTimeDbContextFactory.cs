using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AutoMarket.API;

/// <summary>
/// Fábrica de tiempo de diseño para EF Core (comando "dotnet ef migrations").
/// Solo se usa para generar migraciones: no conecta a la base de datos real,
/// por eso el connection string aquí es un marcador de posición.
/// </summary>
public class AutoMarketDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=design_dummy;Username=postgres;Password=postgres")
            .Options;

        return new ApplicationDbContext(options);
    }
}