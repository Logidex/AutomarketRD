using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Auth;
using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Application.Helpers;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;
using AutoMarket.Core.Interfaces;
using BCrypt.Net;
using Microsoft.Extensions.Logging;

namespace AutoMarket.Application.Services;

public class AuthService : IAuthService
{
    private const int PASSWORD_LONGITUD_MINIMA = 6;
    private static readonly TimeSpan VIGENCIA_CODIGO = TimeSpan.FromMinutes(15);

    private readonly IUsuarioRepository _repository;
    private readonly ITokenService _tokenService;
    private readonly ISuscripcionService _suscripcionService;
    private readonly IEmailSenderService _emailSender;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUsuarioRepository repository,
        ITokenService tokenService,
        ISuscripcionService suscripcionService,
        IEmailSenderService emailSender,
        ILogger<AuthService> logger)
    {
        _repository = repository;
        _tokenService = tokenService;
        _suscripcionService = suscripcionService;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<(bool Exito, string Mensaje)> RegistrarUsuarioAsync(RegistroDto dto)
    {
        var existeEmail = await _repository.ExisteEmailAsync(dto.Email);

        if (existeEmail) return (false, "El correo electrónico ya está registrado.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

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
        bool passwordValido = BCrypt.Net.BCrypt.Verify(
            dto.Password,
            usuario.PasswordHash
        );

        if (!passwordValido)
        {
            throw new UnauthorizedAccessException(
                "Correo electrónico o contraseña incorrectos."
            );
        }

        // 4. Generar el token
        var token = _tokenService.GenerarToken(usuario);

        // 5. Devolver solamente los datos públicos del usuario
        return new LoginResultDto
        {
            Exito = true,
            Mensaje = "Inicio de sesión exitoso.",
            Token = token,
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

    // ==========================================
    // RECUPERACIÓN DE CONTRASEÑA (olvidada)
    // ==========================================
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

        usuario.CambiarPassword(BCrypt.Net.BCrypt.HashPassword(dto.NuevaPassword));

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
}

