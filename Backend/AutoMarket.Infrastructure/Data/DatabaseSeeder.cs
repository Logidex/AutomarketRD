using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Interfaces;
using Microsoft.Extensions.Configuration;
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
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        await SeedAdminAsync(usuarioRepository, config);
        await SeedPlanesCatalogoAsync(scope.ServiceProvider);
    }

    private static async Task SeedAdminAsync(IUsuarioRepository usuarioRepository, IConfiguration config)
    {
        // Las credenciales del admin se leen de configuración (variables de entorno:
        // Admin__Email y Admin__Password). Así nunca quedan hardcodeadas en el código.
        var adminEmail = config["Admin:Email"] ?? "admin@automarket.do";
        var adminPassword = config["Admin:Password"];

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            Console.WriteLine("[Seeder] Usuario admin inicial omitido: configura Admin__Password (Admin:Password) para crearlo.");
            return;
        }

        var adminExiste = await usuarioRepository.ExisteEmailAsync(adminEmail);

        if (!adminExiste)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);

            var adminUser = Usuario.CrearAdministradorInterno(
                nombre: "Administrador",
                apellido: "Supremo",
                email: adminEmail,
                passwordHash: passwordHash
            );

            await usuarioRepository.CrearUsuarioAsync(adminUser);
            await usuarioRepository.GuardarCambiosAsync();

            Console.WriteLine("[Seeder] Usuario admin inicial creado desde configuración.");
            return;
        }

        // Rotación: si Admin__RotatePassword=="true" y hay contraseña configurada,
        // reemplazamos el password hash del admin existente. Sirve para invalidar
        // contraseñas hardcodeadas que hayan quedado en entornos antiguos.
        var rotarPassword = string.Equals(config["Admin:RotatePassword"], "true", StringComparison.OrdinalIgnoreCase);
        if (rotarPassword && !string.IsNullOrWhiteSpace(adminPassword))
        {
            var nuevoHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);
            var actualizado = await usuarioRepository.ActualizarContrasenaAsync(adminEmail, nuevoHash);

            Console.WriteLine(actualizado
                ? "[Seeder] Contraseña del admin rotada desde configuración."
                : "[Seeder] No se encontró el admin para rotar su contraseña.");
        }
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
