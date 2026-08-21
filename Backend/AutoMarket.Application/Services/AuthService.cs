using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Auth;
using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Application.Helpers;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;
using AutoMarket.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AutoMarket.Application.Services;

/// <summary>
/// Servicio para manejar Auth.
/// </summary>
public class AuthService : IAuthService
{
    private const int PASSWORD_LONGITUD_MINIMA = 8;
    private static readonly TimeSpan VIGENCIA_CODIGO = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan VIGENCIA_CONFIRMACION_EMAIL = TimeSpan.FromDays(2);
    private static readonly TimeSpan VIGENCIA_REFRESH_TOKEN = TimeSpan.FromDays(14);

    private readonly IUsuarioRepository _repository;
    private readonly ITokenService _tokenService;
    private readonly ISuscripcionService _suscripcionService;
    private readonly IEmailSenderService _emailSender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly IRefreshTokenRepository _refreshTokens;

/// <summary>
/// Inicializa una nueva instancia de la clase AuthService.
/// </summary>
    public AuthService(
        IUsuarioRepository repository,
        ITokenService tokenService,
        ISuscripcionService suscripcionService,
        IEmailSenderService emailSender,
        IConfiguration configuration,
        ILogger<AuthService> logger,
        IRefreshTokenRepository refreshTokens)
    {
        _repository = repository;
        _tokenService = tokenService;
        _suscripcionService = suscripcionService;
        _emailSender = emailSender;
        _configuration = configuration;
        _logger = logger;
        _refreshTokens = refreshTokens;
    }

    public async Task<(bool Exito, string Mensaje)> RegistrarUsuarioAsync(RegistroDto dto)
    {
        var existeEmail = await _repository.ExisteEmailAsync(dto.Email);

        if (existeEmail) return (false, "El correo electrónico ya está registrado.");

        var passwordHash = HasherPassword.Hash(dto.Password);

        var nuevoUsuario = new Usuario(
            nombre: dto.Nombre,
            apellido: dto.Apellido,
            email: dto.Email.ToLowerInvariant(),
            passwordHash: passwordHash,
            rol: dto.Rol,
            telefonoPersonal: dto.TelefonoPersonal
        );

        if (nuevoUsuario.Rol == "Dealer")
        {
            if (string.IsNullOrWhiteSpace(dto.NombreAgencia) || string.IsNullOrWhiteSpace(dto.AgenciaRNC))
            {
                return (false, "Los datos de la agencia y el RNC son obligatorios para cuentas tipo Dealer.");
            }

            nuevoUsuario.CrearPerfilDealer(
                nombreAgencia: dto.NombreAgencia,
                agenciaRNC: dto.AgenciaRNC,
                ubicacion: dto.UbicacionAgencia,
                telefonoAgencia: dto.TelefonoAgencia
            );
        }

        await _repository.CrearUsuarioAsync(nuevoUsuario);

        if (nuevoUsuario.Rol == "Dealer")
        {
            var perfilDealerId = nuevoUsuario.PerfilDealer!.UsuarioId;

            await _suscripcionService.AsignarPlanInicialAsync(
                perfilDealerId,
                PlanNivel.Gratis,
                CicloFacturacion.Mensual
            );

            await GenerarYEnviarConfirmacionEmailAsync(nuevoUsuario);

            return (true, "Usuario registrado exitosamente. Te enviamos un correo para confirmar tu dirección de email.");
        }

        return (true, "Usuario registrado exitosamente");
    }

    public async Task<LoginResultDto> LoginAsync(LoginDto dto)
    {
        // 1. Buscar el usuario por email
        var usuario = await _repository.ObtenerPorEmailAsync(dto.Email);

        if (usuario == null)
        {
            throw new UnauthorizedAccessException(
                "Correo electrónico o contraseña incorrectos."
            );
        }

        // 2. Verificar si la cuenta está activa
        if (!usuario.IsActivo)
        {
            throw new UnauthorizedAccessException(
                "Tu cuenta ha sido suspendida por un administrador. " +
                "Contacta a soporte para más información."
            );
        }

        // 3. Verificar la contraseña
        bool passwordValido = HasherPassword.Verificar(
            dto.Password,
            usuario.PasswordHash
        );

        if (!passwordValido)
        {
            throw new UnauthorizedAccessException(
                "Correo electrónico o contraseña incorrectos."
            );
        }

        // 4. Generar el token de acceso y el refresh token de la sesión
        var token = _tokenService.GenerarToken(usuario);
        var refreshToken = await EmitirRefreshTokenAsync(usuario.UsuarioId);

        // 5. Devolver solamente los datos públicos del usuario
        return new LoginResultDto
        {
            Exito = true,
            Mensaje = "Inicio de sesión exitoso.",
            Token = token,
            RefreshToken = refreshToken,
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

    public async Task<LoginResultDto> RefrescarSesionAsync(string refreshTokenCrudo)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenCrudo))
            throw new UnauthorizedAccessException("Sesión inválida.");

        var hash = _tokenService.HashRefreshToken(refreshTokenCrudo);

        var token = await _refreshTokens.ObtenerPorHashAsync(hash);

        if (token == null)
            throw new UnauthorizedAccessException("Sesión inválida.");

        if (!token.EstaActivo)
        {
            // Reuso de un token ya rotado/expirado: señal de robo.
            // Se revoca toda la familia activa del usuario como contención.
            await _refreshTokens.RevocarActivosDeUsuarioAsync(token.UsuarioId);
            await _refreshTokens.GuardarCambiosAsync();
            _logger.LogWarning(
                "Reuso de refresh token detectado (usuario {UsuarioId}); familia revocada.",
                token.UsuarioId);

            throw new UnauthorizedAccessException("Sesión inválida.");
        }

        var usuario = await _repository.ObtenerPorIdAsync(token.UsuarioId);

        if (usuario == null || !usuario.IsActivo)
            throw new UnauthorizedAccessException("Sesión inválida.");

        // Rotación: el token usado muere apuntando a su reemplazo.
        var nuevoRefresh = _tokenService.GenerarRefreshToken();
        var nuevoHash = _tokenService.HashRefreshToken(nuevoRefresh);

        token.Revocar(nuevoHash);
        await _refreshTokens.AgregarAsync(new RefreshToken(
            usuario.UsuarioId,
            nuevoHash,
            DateTime.UtcNow,
            DateTime.UtcNow.Add(VIGENCIA_REFRESH_TOKEN)));
        await _refreshTokens.GuardarCambiosAsync();

        return new LoginResultDto
        {
            Exito = true,
            Mensaje = "Sesión renovada.",
            Token = _tokenService.GenerarToken(usuario),
            RefreshToken = nuevoRefresh,
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

    public async Task RevocarSesionAsync(string? refreshTokenCrudo)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenCrudo))
            return;

        var hash = _tokenService.HashRefreshToken(refreshTokenCrudo);

        var token = await _refreshTokens.ObtenerPorHashAsync(hash);

        if (token != null && token.EstaActivo)
        {
            token.Revocar();
            await _refreshTokens.GuardarCambiosAsync();
        }
    }

    /// <summary>Crea y persiste un refresh token nuevo para el usuario.</summary>
    private async Task<string> EmitirRefreshTokenAsync(int usuarioId)
    {
        var crudo = _tokenService.GenerarRefreshToken();

        await _refreshTokens.AgregarAsync(new RefreshToken(
            usuarioId,
            _tokenService.HashRefreshToken(crudo),
            DateTime.UtcNow,
            DateTime.UtcNow.Add(VIGENCIA_REFRESH_TOKEN)));
        await _refreshTokens.GuardarCambiosAsync();

        return crudo;
    }

    // ==========================================
    // RECUPERACIÓN DE CONTRASEÑA (olvidada)
    // ==========================================
/// <summary>
/// SolicitarRecuperacionAsync Solicitar recuperacion async. Parámetros: Parámetro email (string). Retorna: Task.
/// </summary>
    public async Task SolicitarRecuperacionAsync(string email)
    {
        // No revelar si el correo existe: siempre se responde con el mismo mensaje.
        var usuario = await _repository.ObtenerPorEmailParaEscrituraAsync(email.Trim().ToLowerInvariant());

        if (usuario == null || !usuario.IsActivo)
            return;

        var codigo = CodigoUtil.GenerarCodigoNumerico();

        usuario.EstablecerCodigoRecuperacion(
            CodigoUtil.HashCodigo(codigo),
            DateTime.UtcNow.Add(VIGENCIA_CODIGO));

        await _repository.GuardarCambiosAsync();

        try
        {
            await _emailSender.EnviarCorreoAsync(
                usuario.Email,
                "Recupera tu contraseña en AutoMarket RD",
                "<p>Hola <strong>" + usuario.Nombre + "</strong>,</p>" +
                "<p>Usa este código para restablecer tu contraseña:</p>" +
                "<h2 style='letter-spacing:6px'>" + codigo + "</h2>" +
                "<p>El código expira en 15 minutos. Si no solicitaste esto, ignora este correo.</p>");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando correo de recuperación a {Email}", usuario.Email);
        }
    }

/// <summary>
/// RestablecerPasswordAsync Restablecer password async. Parámetros: Parámetro dto (RestablecerPasswordDto). Retorna: Task.
/// </summary>
    public async Task RestablecerPasswordAsync(RestablecerPasswordDto dto)
    {
        var usuario = await _repository.ObtenerPorEmailParaEscrituraAsync(dto.Email.Trim().ToLowerInvariant())
            ?? throw new UnauthorizedAccessException("No pudimos validar tu información. Solicita un nuevo código.");

        if (!usuario.IsActivo)
            throw new UnauthorizedAccessException("Tu cuenta está suspendida. Contacta a soporte.");

        if (dto.NuevaPassword.Length < PASSWORD_LONGITUD_MINIMA)
            throw new BusinessRuleException($"La nueva contraseña debe tener al menos {PASSWORD_LONGITUD_MINIMA} caracteres.");

        if (!usuario.AplicarCodigoRecuperacionSiValido(CodigoUtil.HashCodigo(dto.Codigo), DateTime.UtcNow))
            throw new BusinessRuleException("El código es inválido o ha expirado. Solicita un nuevo código.");

        usuario.CambiarPassword(HasherPassword.Hash(dto.NuevaPassword));

        // Seguridad: la contraseña cambió; se cierran todas las sesiones activas
        await _refreshTokens.RevocarActivosDeUsuarioAsync(usuario.UsuarioId);

        await _repository.GuardarCambiosAsync();

        try
        {
            await _emailSender.EnviarCorreoAsync(
                usuario.Email,
                "Tu contraseña ha sido restablecida",
                "<p>Hola <strong>" + usuario.Nombre + "</strong>,</p>" +
                "<p>Tu contraseña fue restablecida exitosamente.</p>" +
                "<p>Si no realizaste este cambio, contacta a soporte de inmediato.</p>");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notificando restablecimiento a {Email}", usuario.Email);
        }
    }

    // ==========================================
    // CONFIRMACIÓN DE CORREO EN EL ALTA DE CUENTA (Dealer)
    // ==========================================
    private async Task GenerarYEnviarConfirmacionEmailAsync(Usuario usuario)
    {
        if (usuario.EmailConfirmado)
            return;

        var token = CodigoUtil.GenerarTokenConfirmacion();

        usuario.EstablecerConfirmacionEmail(
            CodigoUtil.HashCodigo(token),
            DateTime.UtcNow.Add(VIGENCIA_CONFIRMACION_EMAIL));

        await _repository.GuardarCambiosAsync();

        var frontendUrl = _configuration["App:FrontendUrl"] ?? "http://localhost:5173";
        var enlace = $"{frontendUrl.TrimEnd('/')}/confirmar-correo?token={token}";

        try
        {
            await _emailSender.EnviarCorreoAsync(
                usuario.Email,
                "Confirma tu correo en AutoMarket RD",
                "<p>Hola <strong>" + usuario.Nombre + "</strong>,</p>" +
                "<p>Gracias por crear tu cuenta Dealer en AutoMarket RD.</p>" +
                "<p>Para activar tu correo y poder obtener la insignia de <strong>Dealer Verificado</strong>, confirma tu dirección de correo:</p>" +
                "<p style='text-align:center'><a href='" + enlace + "' style='background-color:#2563eb;color:#ffffff;padding:12px 24px;border-radius:8px;text-decoration:none;font-weight:bold'>Confirmar mi correo</a></p>" +
                "<p>El enlace expira en 48 horas. Si no creaste esta cuenta, ignora este correo.</p>" +
                "<p>Si el botón no funciona, copia este enlace en tu navegador: <br/>" + enlace + "</p>");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando confirmación de correo a {Email}", usuario.Email);
        }
    }

    /// <summary>
    /// Confirma el correo de una cuenta usando el token del enlace enviado al registrarse.
    /// </summary>
    public async Task ConfirmarCorreoAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new BusinessRuleException("El enlace de confirmación es inválido.");

        var codigoHash = CodigoUtil.HashCodigo(token);

        var usuario = await _repository.ObtenerPorCodigoConfirmacionEmailAsync(codigoHash)
            ?? throw new BusinessRuleException("El enlace de confirmación es inválido o ya fue utilizado.");

        if (!usuario.ConfirmarEmailSiValido(codigoHash, DateTime.UtcNow))
            throw new BusinessRuleException("El enlace de confirmación ha expirado. Solicita uno nuevo.");

        await _repository.GuardarCambiosAsync();
    }

    /// <summary>
    /// Reenvía el correo de confirmación a un dealer cuyo correo aún no está confirmado.
    /// Responde siempre igual para no revelar si un correo está registrado.
    /// </summary>
    public async Task ReenviarConfirmacionCorreoAsync(string email)
    {
        var usuario = await _repository.ObtenerPorEmailParaEscrituraAsync(email.Trim().ToLowerInvariant());

        if (usuario == null || usuario.EmailConfirmado || usuario.Rol != "Dealer" || !usuario.IsActivo)
            return;

        await GenerarYEnviarConfirmacionEmailAsync(usuario);
    }
}


