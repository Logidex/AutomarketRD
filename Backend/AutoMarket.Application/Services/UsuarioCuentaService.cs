using AutoMarket.Application.Constants;
using AutoMarket.Application.DTOs.Admin;
using AutoMarket.Application.DTOs.Auth;
using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Application.Helpers;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Constants;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;
using AutoMarket.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AutoMarket.Application.Services;

/// <summary>
/// Servicio para manejar UsuarioCuenta.
/// </summary>
public class UsuarioCuentaService : IUsuarioCuentaService
{
    private const int PASSWORD_LONGITUD_MINIMA = 6;
    private static readonly TimeSpan VIGENCIA_CODIGO = TimeSpan.FromMinutes(15);

    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISuscripcionService _suscripcionService;
    private readonly ITokenService _tokenService;
    private readonly IEmailSenderService _emailSender;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly ILogger<UsuarioCuentaService> _logger;
    private readonly IConfiguration _configuration;

/// <summary>
/// Inicializa una nueva instancia de la clase UsuarioCuentaService.
/// </summary>
    public UsuarioCuentaService(
        IUsuarioRepository usuarioRepository,
        ISuscripcionService suscripcionService,
        ITokenService tokenService,
        IEmailSenderService emailSender,
        IRefreshTokenRepository refreshTokens,
        ILogger<UsuarioCuentaService> logger,
        IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository;
        _suscripcionService = suscripcionService;
        _tokenService = tokenService;
        _emailSender = emailSender;
        _refreshTokens = refreshTokens;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<UsuarioCuentaDto> ObtenerCuentaAsync(int usuarioId)
    {
        var usuario = await ObtenerUsuarioAsync(usuarioId);
        return MapearCuenta(usuario);
    }

    public async Task<UsuarioCuentaDto> ActualizarDatosAsync(int usuarioId, ActualizarDatosDto dto)
    {
        var usuario = await ObtenerUsuarioAsync(usuarioId);

        usuario.ActualizarDatos(dto.Nombre, dto.Apellido, dto.TelefonoPersonal);

        await _usuarioRepository.GuardarCambiosAsync();

        return MapearCuenta(usuario);
    }

    public async Task<LoginResultDto> AscenderRolAsync(int usuarioId, AscenderRolDto dto)
    {
        // Cargamos el perfil comercial (y su suscripción) para poder reutilizarlo
        // al ascender: un Vendedor ya posee un PerfilDealer que debe actualizarse
        // en lugar de reinsertarse (evita la violación de clave primaria).
        var usuario = await _usuarioRepository.ObtenerDealerConPerfilPorIdAsync(usuarioId)
            ?? throw new KeyNotFoundException("No se encontró la cuenta del usuario.");

        var nuevoRol = dto.NuevoRol?.Trim();

        if (nuevoRol == "Vendedor")
        {
            usuario.ConvertirAVendedor();
            await _usuarioRepository.GuardarCambiosAsync();

            return GenerarSesion(usuario, "Tu cuenta ahora es de tipo Vendedor.");
        }

        if (nuevoRol == "Dealer")
        {
            if (string.IsNullOrWhiteSpace(dto.NombreAgencia) || string.IsNullOrWhiteSpace(dto.AgenciaRNC))
                throw new BusinessRuleException("Los datos de la agencia y el RNC son obligatorios para cuentas tipo Dealer.");

            usuario.ConvertirADealer(
                nombreAgencia: dto.NombreAgencia!.Trim(),
                agenciaRNC: dto.AgenciaRNC!.Trim(),
                ubicacion: dto.UbicacionAgencia ?? string.Empty,
                telefonoAgencia: dto.TelefonoAgencia ?? string.Empty
            );

            await _usuarioRepository.GuardarCambiosAsync();

            // Un Vendedor ya posee perfil y suscripción Gratis de su registro;
            // solo se asigna el plan inicial si el perfil aún no tiene uno.
            if (usuario.PerfilDealer!.Suscripcion == null)
            {
                var perfilDealerId = usuario.PerfilDealer.UsuarioId;
                await _suscripcionService.AsignarPlanInicialAsync(perfilDealerId, PlanNivel.Gratis, CicloFacturacion.Mensual);
            }

            return GenerarSesion(usuario, "Tu cuenta ahora es de tipo Dealer. Se te asignó el plan Gratis.");
        }

        throw new BusinessRuleException("El rol de destino no es válido. Solo puedes ascender a Vendedor o Dealer.");
    }

    /// <summary>
    /// Cambio de rol efectuado por el administrador. Permite ascender o degradar
    /// una cuenta a Comprador/Vendedor/Dealer. Cuando se deja de ser Dealer se
    /// elimina el perfil comercial (y con él, su suscripción y pagos asociados).
    /// </summary>
    public async Task<UsuarioCuentaDto> CambiarRolAdminAsync(int usuarioId, CambiarRolAdminDto dto)
    {
        var usuario = await _usuarioRepository.ObtenerDealerConPerfilPorIdAsync(usuarioId)
            ?? throw new KeyNotFoundException("No se encontró la cuenta del usuario.");

        var nuevoRol = dto.NuevoRol?.Trim();

        if (nuevoRol != "Comprador" && nuevoRol != "Vendedor" && nuevoRol != "Dealer")
            throw new BusinessRuleException("El rol de destino no es válido. Usa Comprador, Vendedor o Dealer.");

        if (usuario.Rol == nuevoRol)
            throw new BusinessRuleException("El usuario ya posee ese rol.");

        if (nuevoRol == "Dealer")
        {
            if (string.IsNullOrWhiteSpace(dto.NombreAgencia) || string.IsNullOrWhiteSpace(dto.AgenciaRNC))
                throw new BusinessRuleException("Para convertir en Dealer son obligatorios el nombre de la agencia y el RNC.");

            usuario.ConvertirADealer(
                nombreAgencia: dto.NombreAgencia!.Trim(),
                agenciaRNC: dto.AgenciaRNC!.Trim(),
                ubicacion: dto.UbicacionAgencia ?? string.Empty,
                telefonoAgencia: dto.TelefonoAgencia ?? string.Empty);

            await _usuarioRepository.GuardarCambiosAsync();

            // Un Vendedor ya posee perfil y suscripción Gratis de su registro;
            // solo se asigna el plan inicial si el perfil aún no tiene uno.
            if (usuario.PerfilDealer!.Suscripcion == null)
            {
                var perfilDealerId = usuario.PerfilDealer.UsuarioId;
                await _suscripcionService.AsignarPlanInicialAsync(perfilDealerId, PlanNivel.Gratis, CicloFacturacion.Mensual);
            }
        }
        else
        {
            if (usuario.PerfilDealer != null)
            {
                await _usuarioRepository.EliminarPerfilDealerAsync(usuarioId);
                usuario.QuitarPerfilDealer();
            }

            usuario.FijarRolAdmin(nuevoRol);
            await _usuarioRepository.GuardarCambiosAsync();
        }

        return MapearCuenta(usuario);
    }

/// <summary>
/// SolicitarCambioPasswordAsync Solicitar cambio password async. Parámetros: Parámetro usuarioId (int), Parámetro dto (SolicitarCambioPasswordDto). Retorna: Task.
/// </summary>
    public async Task SolicitarCambioPasswordAsync(int usuarioId, SolicitarCambioPasswordDto dto)
    {
        var usuario = await ObtenerUsuarioAsync(usuarioId);

        // Confirmación: solo el dueño conoce la contraseña actual
        if (!HasherPassword.Verificar(dto.PasswordActual, usuario.PasswordHash))
            throw new UnauthorizedAccessException("La contraseña actual es incorrecta.");

        if (dto.NuevaPassword.Length < PASSWORD_LONGITUD_MINIMA)
            throw new BusinessRuleException($"La nueva contraseña debe tener al menos {PASSWORD_LONGITUD_MINIMA} caracteres.");

        if (string.Equals(dto.PasswordActual, dto.NuevaPassword, StringComparison.Ordinal))
            throw new BusinessRuleException("La nueva contraseña debe ser diferente a la actual.");

        // Límite de correos por usuario: cooldown por tipo + tope diario
        var espera = usuario.TryRegistrarEnvioEmail(
            TiposEmail.CambioPassword,
            DateTime.UtcNow,
            ReglasEmail.CooldownPorTipo,
            ReglasEmail.TopeDiario);

        if (espera is int minutosEspera)
            throw new BusinessRuleException(
                $"Ya se envió un código de confirmación recientemente. Espera {minutosEspera} minuto(s) para solicitar otro.");

        var nuevoHash = HasherPassword.Hash(dto.NuevaPassword);
        var codigo = CodigoUtil.GenerarCodigoNumerico();

        usuario.EstablecerCambioPassword(
            nuevoHash,
            CodigoUtil.HashCodigo(codigo),
            DateTime.UtcNow.Add(VIGENCIA_CODIGO));

        await _usuarioRepository.GuardarCambiosAsync();

        NotificarPorCorreo(
            usuario.Email,
            "Confirma el cambio de contraseña en AutoMarket RD",
            "<p>Hola <strong>" + usuario.Nombre + "</strong>,</p>" +
            "<p>Usa este código para confirmar el cambio de tu contraseña:</p>" +
            "<h2 style='letter-spacing:6px'>" + codigo + "</h2>" +
            "<p>El código expira en 15 minutos. Si no solicitaste este cambio, ignora este correo.</p>");
    }

/// <summary>
/// ConfirmarCambioPasswordAsync Confirmar cambio password async. Parámetros: Parámetro usuarioId (int), Parámetro dto (ConfirmarPasswordDto). Retorna: Task.
/// </summary>
    public async Task ConfirmarCambioPasswordAsync(int usuarioId, ConfirmarPasswordDto dto)
    {
        var usuario = await ObtenerUsuarioAsync(usuarioId);

        var codigoHash = CodigoUtil.HashCodigo(dto.Codigo);

        if (!usuario.AplicarCambioPasswordSiValido(codigoHash, DateTime.UtcNow))
            throw new BusinessRuleException("El código es inválido o ha expirado. Solicita un nuevo código.");

        // La identidad quedó demostrada con el código: se limpia el bloqueo por intentos fallidos
        usuario.ReiniciarIntentosFallidos();

        // Seguridad: la contraseña cambió; se cierran todas las sesiones activas
        await _refreshTokens.RevocarActivosDeUsuarioAsync(usuario.UsuarioId);

        await _usuarioRepository.GuardarCambiosAsync();

        NotificarPorCorreo(
            usuario.Email,
            "Tu contraseña en AutoMarket RD ha sido cambiada",
            "<p>Hola <strong>" + usuario.Nombre + "</strong>,</p>" +
            "<p>Te confirmamos que tu contraseña fue actualizada correctamente.</p>" +
            "<p>Si no realizaste este cambio, contacta a soporte de inmediato.</p>");
    }

/// <summary>
/// SolicitarCambioEmailAsync Solicitar cambio email async. Parámetros: Parámetro usuarioId (int), Parámetro dto (SolicitarCambioEmailDto). Retorna: Task.
/// </summary>
    public async Task SolicitarCambioEmailAsync(int usuarioId, SolicitarCambioEmailDto dto)
    {
        var usuario = await ObtenerUsuarioAsync(usuarioId);

        // Confirmación: solo el dueño conoce la contraseña actual
        if (!HasherPassword.Verificar(dto.PasswordActual, usuario.PasswordHash))
            throw new UnauthorizedAccessException("La contraseña actual es incorrecta.");

        var nuevoEmail = dto.NuevoEmail.Trim().ToLowerInvariant();

        if (usuario.Email == nuevoEmail)
            throw new BusinessRuleException("El nuevo correo es igual al correo actual.");

        if (await _usuarioRepository.ExisteEmailAsync(nuevoEmail))
            throw new BusinessRuleException("Ese correo electrónico ya está registrado.");

        // Límite de correos por usuario: cooldown por tipo + tope diario
        var espera = usuario.TryRegistrarEnvioEmail(
            TiposEmail.CambioEmail,
            DateTime.UtcNow,
            ReglasEmail.CooldownPorTipo,
            ReglasEmail.TopeDiario);

        if (espera is int minutosEspera)
            throw new BusinessRuleException(
                $"Ya se envió un código de confirmación recientemente. Espera {minutosEspera} minuto(s) para solicitar otro.");

        var codigo = CodigoUtil.GenerarCodigoNumerico();

        usuario.EstablecerCambioEmail(
            nuevoEmail,
            CodigoUtil.HashCodigo(codigo),
            DateTime.UtcNow.Add(VIGENCIA_CODIGO));

        await _usuarioRepository.GuardarCambiosAsync();

        NotificarPorCorreo(
            nuevoEmail,
            "Confirma tu nuevo correo en AutoMarket RD",
            "<p>Hola <strong>" + usuario.Nombre + "</strong>,</p>" +
            "<p>Usa este código para confirmar tu nuevo correo electrónico:</p>" +
            "<h2 style='letter-spacing:6px'>" + codigo + "</h2>" +
            "<p>El código expira en 15 minutos. Si no solicitaste este cambio, ignora este correo.</p>");
    }

/// <summary>
/// ConfirmarCambioEmailAsync Confirmar cambio email async. Parámetros: Parámetro usuarioId (int), Parámetro dto (ConfirmarEmailDto). Retorna: Task.
/// </summary>
    public async Task ConfirmarCambioEmailAsync(int usuarioId, ConfirmarEmailDto dto)
    {
        var usuario = await ObtenerUsuarioAsync(usuarioId);

        var codigoHash = CodigoUtil.HashCodigo(dto.Codigo);

        if (!usuario.AplicarCambioEmailSiValido(codigoHash, DateTime.UtcNow))
            throw new BusinessRuleException("El código es inválido o ha expirado. Solicita un nuevo código.");

        await _usuarioRepository.GuardarCambiosAsync();

        NotificarPorCorreo(
            usuario.Email,
            "Tu correo en AutoMarket RD ha sido actualizado",
            "<p>Hola <strong>" + usuario.Nombre + "</strong>,</p>" +
            "<p>Tu correo electrónico fue actualizado exitosamente.</p>");
    }

    private async Task<Usuario> ObtenerUsuarioAsync(int usuarioId)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId);

        if (usuario == null)
            throw new KeyNotFoundException("No se encontró la cuenta del usuario.");

        return usuario;
    }

    private static UsuarioCuentaDto MapearCuenta(Usuario usuario)
    {
        return new UsuarioCuentaDto
        {
            UsuarioId = usuario.UsuarioId,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Email = usuario.Email,
            EmailConfirmado = usuario.EmailConfirmado,
            TelefonoPersonal = usuario.TelefonoPersonal,
            Rol = usuario.Rol
        };
    }

    // Tras el ascenso el rol cambió: hay que re-emitir el token con el nuevo rol
    // y devolver la sesión actualizada igual que hace el login.
    private LoginResultDto GenerarSesion(Usuario usuario, string mensaje)
    {
        return new LoginResultDto
        {
            Exito = true,
            Mensaje = mensaje,
            Token = _tokenService.GenerarToken(usuario),
            Usuario = new UsuarioAuthDto
            {
                UsuarioId = usuario.UsuarioId,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido ?? string.Empty,
                Email = usuario.Email,
                Rol = usuario.Rol
            }
        };
    }

    // El correo no debe impedir la operación principal
    private void NotificarPorCorreo(string destinatario, string asunto, string cuerpoHtml)
    {
        try
        {
            var cuerpoFinal = PlantillaCorreoHelper.Envolver(
                _configuration["App:FrontendUrl"],
                null,
                cuerpoHtml);

            _emailSender.EnviarCorreoAsync(destinatario, asunto, cuerpoFinal)
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo notificar por correo a {Destinatario}", destinatario);
        }
    }
}
