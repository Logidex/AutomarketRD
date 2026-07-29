using AutoMarket.Core.Entities;
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
    }
}
