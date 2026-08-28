using AutoMarket.Application.Helpers;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        await SeedCuponBienvenidaAsync(scope.ServiceProvider, config);
        await SeedEncuestaAsync(scope.ServiceProvider);
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
            var passwordHash = HasherPassword.Hash(adminPassword);

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
            var nuevoHash = HasherPassword.Hash(adminPassword);
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
            new { Nivel = PlanNivel.Gratis, Nombre = "Plan Gratis", Descripcion = "Para que pruebes la plataforma: publica 1 vehículo sin costo.", PrecioMensual = 0m, DescTrim = 0m, DescAnual = 0m },
            new { Nivel = PlanNivel.Basico, Nombre = "Plan Básico", Descripcion = "Para vendedores que inician: hasta 15 vehículos publicados.", PrecioMensual = 999m, DescTrim = 7m, DescAnual = 15m },
            new { Nivel = PlanNivel.Pro, Nombre = "Plan Pro", Descripcion = "Para vendedores activos: hasta 50 vehículos y 5 destacados en la portada.", PrecioMensual = 2499m, DescTrim = 7m, DescAnual = 15m },
            new { Nivel = PlanNivel.Elite, Nombre = "Plan Elite", Descripcion = "El máximo poder: hasta 150 vehículos y 20 destacados en la portada.", PrecioMensual = 4999m, DescTrim = 7m, DescAnual = 15m }
        };

        foreach (var p in planes)
        {
            var existente = await planRepository.ObtenerPorNivelAsync(p.Nivel);

            if (existente != null)
            {
                // No sobreescribir planes existentes: precios, descuentos, nombre,
                // descripción y cuota de destacados son editables desde el panel de
                // administración. El seeder solo garantiza que los planes base existan.
                continue;
            }

            var cuotaDestacados = PlanConfig.CuotaDestacados(p.Nivel);

            await planRepository.AgregarAsync(new PlanCatalogo
            {
                Nivel = p.Nivel,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                PrecioMensual = p.PrecioMensual,
                DescuentoTrimestralPorcentaje = p.DescTrim,
                DescuentoAnualPorcentaje = p.DescAnual,
                LimiteAnuncios = PlanConfig.LimiteAnuncios(p.Nivel),
                MaxFotos = PlanConfig.MaxFotos(p.Nivel),
                DiasVigencia = PlanConfig.DiasVigencia(p.Nivel),
                CuotaDestacados = cuotaDestacados,
                Activo = true
            });
        }
    }

    /// <summary>
    /// Crea el cupón de bienvenida para dealers (Pro por N días, tope M usos)
    /// si aún no existe. Código/días/tope configurables vía
    /// Cupon__Bienvenida__Codigo / __Dias / __MaximoUsos.
    /// </summary>
    private static async Task SeedCuponBienvenidaAsync(IServiceProvider serviceProvider, IConfiguration config)
    {
        var cuponRepository = serviceProvider.GetRequiredService<ICuponRepository>();

        var codigo = config["Cupon:Bienvenida:Codigo"] ?? "PRO15BIENVENIDA";
        var dias = config.GetValue("Cupon:Bienvenida:Dias", 15);
        var maximoUsos = config.GetValue("Cupon:Bienvenida:MaximoUsos", 15);

        var existente = await cuponRepository.ObtenerPorCodigoAsync(codigo);

        if (existente != null)
        {
            return;
        }

        var creado = await cuponRepository.AgregarAsync(new Cupon(codigo, PlanNivel.Pro, dias, maximoUsos));

        Console.WriteLine($"[Seeder] Cupón de bienvenida creado: {creado.Codigo} ({creado.Dias} días Pro, tope {creado.MaximoUsos} usos).");
    }

    /// <summary>
    /// Crea la encuesta fija de satisfacción si no existe ninguna activa.
    /// V1: preguntas fijas en código (2 de escala + 1 abierta).
    /// </summary>
    private static async Task SeedEncuestaAsync(IServiceProvider serviceProvider)
    {
        var encuestaRepository = serviceProvider.GetRequiredService<IEncuestaRepository>();

        var activa = await encuestaRepository.ObtenerActivaAsync();

        if (activa != null)
        {
            return;
        }

        var encuesta = new Encuesta(
            "¿Cómo es tu experiencia en AutoMarket RD?",
            "Tu opinión nos ayuda a mejorar el marketplace. Toma menos de 1 minuto.");

        encuesta.AgregarPregunta(
            "¿Qué tan satisfecho estás con la plataforma en general?",
            TipoPreguntaEncuesta.Escala);

        encuesta.AgregarPregunta(
            "¿Qué tan fácil es encontrar o publicar vehículos?",
            TipoPreguntaEncuesta.Escala);

        encuesta.AgregarPregunta(
            "¿Qué cambiarías o agregarías? (opcional)",
            TipoPreguntaEncuesta.Abierta);

        await encuestaRepository.AgregarAsync(encuesta);

        Console.WriteLine("[Seeder] Encuesta de satisfacción creada (2 escala + 1 abierta).");
    }
}
