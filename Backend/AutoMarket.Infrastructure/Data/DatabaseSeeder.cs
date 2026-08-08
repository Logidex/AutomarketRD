using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using BCrypt.Net;

namespace AutoMarket.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        // Creamos un "scope" para poder pedirle servicios al contenedor de inyección de dependencias
        using var scope = serviceProvider.CreateScope();
        var usuarioRepository = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();

        // 1. Definir las credenciales de tu Admin Supremo
        var adminEmail = "admin@automarket.do";
        
        // 2. Verificar si ya existe para no duplicarlo cada vez que inicies la API
        var adminExiste = await usuarioRepository.ExisteEmailAsync(adminEmail);

        if (!adminExiste)
        {
            // 3. Hashear la contraseña
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("***REDACTED***");

            // 4. Utilizar tu método de dominio blindado (Separando nombre y apellido)
            var adminUser = Usuario.CrearAdministradorInterno(
                nombre: "Administrador",
                apellido: "Supremo",
                email: adminEmail,
                passwordHash: passwordHash
            );

            // 5. Guardar en la base de datos
            await usuarioRepository.CrearUsuarioAsync(adminUser);
            await usuarioRepository.GuardarCambiosAsync();
        }

        await SeedPlanesCatalogoAsync(scope.ServiceProvider);
    }

    private static async Task SeedPlanesCatalogoAsync(IServiceProvider serviceProvider)
    {
        var planRepository = serviceProvider.GetRequiredService<IPlanCatalogoRepository>();

        var planes = new[]
        {
            new { Nivel = PlanNivel.Gratis, Nombre = "Plan Gratis", Descripcion = "Publiqué para ver el primer vehículo gratis.", PrecioMensual = 0m, DescTrim = 0m, DescAnual = 0m },
            new { Nivel = PlanNivel.Basico, Nombre = "Plan Básico", Descripcion = "Para vendedores que inician con hasta 50 vehículos.", PrecioMensual = 1500m, DescTrim = 7m, DescAnual = 15m },
            new { Nivel = PlanNivel.Pro, Nombre = "Plan Pro", Descripcion = "Para vendedores activos con hasta 200 vehículos.", PrecioMensual = 3000m, DescTrim = 7m, DescAnual = 15m },
            new { Nivel = PlanNivel.Elite, Nombre = "Plan Elite", Descripcion = "El máximo poder para hasta 500 vehículos.", PrecioMensual = 5500m, DescTrim = 7m, DescAnual = 15m }
        };

        foreach (var p in planes)
        {
            var existente = await planRepository.ObtenerPorNivelAsync(p.Nivel);
            if (existente != null)
                continue;

            await planRepository.AgregarAsync(new PlanCatalogo
            {
                Nivel = p.Nivel,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                PrecioMensual = p.PrecioMensual,
                DescuentoTrimestralPorcentaje = p.DescTrim,
                DescuentoAnualPorcentaje = p.DescAnual,
                Activo = true
            });
        }
    }
}
