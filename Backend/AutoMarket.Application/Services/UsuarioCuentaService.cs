using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Application.Helpers;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Exceptions;
using AutoMarket.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AutoMarket.Application.Services;

public class UsuarioCuentaService : IUsuarioCuentaService
{
    private const int PASSWORD_LONGITUD_MINIMA = 6;
    private static readonly TimeSpan VIGENCIA_CODIGO = TimeSpan.FromMinutes(15);

    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IEmailSenderService _emailSender;
    private readonly ILogger<UsuarioCuentaService> _logger;

    public UsuarioCuentaService(
        IUsuarioRepository usuarioRepository,
        IEmailSenderService emailSender,
        ILogger<UsuarioCuentaService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _emailSender = emailSender;
        _logger = logger;
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

    public async Task CambiarPasswordAsync(int usuarioId, CambiarPasswordDto dto)
    {
        var usuario = await ObtenerUsuarioAsync(usuarioId);

        // Confirmación: solo el dueño conoce la contraseña actual
        if (!BCrypt.Net.BCrypt.Verify(dto.PasswordActual, usuario.PasswordHash))
            throw new UnauthorizedAccessException("La contraseña actual es incorrecta.");

        if (dto.NuevaPassword.Length < PASSWORD_LONGITUD_MINIMA)
            throw new BusinessRuleException($"La nueva contraseña debe tener al menos {PASSWORD_LONGITUD_MINIMA} caracteres.");

        if (string.Equals(dto.PasswordActual, dto.NuevaPassword, StringComparison.Ordinal))
            throw new BusinessRuleException("La nueva contraseña debe ser diferente a la actual.");

        var nuevoHash = BCrypt.Net.BCrypt.HashPassword(dto.NuevaPassword);
        usuario.CambiarPassword(nuevoHash);

        await _usuarioRepository.GuardarCambiosAsync();

        NotificarPorCorreo(
            usuario.Email,
            "Tu contraseña de AutoMarket RD ha sido cambiada",
            "<p>Hola <strong>" + usuario.Nombre + "</strong>,</p>" +
            "<p>Te confirmamos que tu contraseña fue actualizada correctamente.</p>" +
            "<p>Si no realizaste este cambio, contacta a soporte de inmediato.</p>");
    }

    public async Task SolicitarCambioEmailAsync(int usuarioId, SolicitarCambioEmailDto dto)
    {
        var usuario = await ObtenerUsuarioAsync(usuarioId);

        // Confirmación: solo el dueño conoce la contraseña actual
        if (!BCrypt.Net.BCrypt.Verify(dto.PasswordActual, usuario.PasswordHash))
            throw new UnauthorizedAccessException("La contraseña actual es incorrecta.");

        var nuevoEmail = dto.NuevoEmail.Trim().ToLowerInvariant();

        if (usuario.Email == nuevoEmail)
            throw new BusinessRuleException("El nuevo correo es igual al correo actual.");

        if (await _usuarioRepository.ExisteEmailAsync(nuevoEmail))
            throw new BusinessRuleException("Ese correo electrónico ya está registrado.");

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

    // El correo no debe impedir la operación principal
    private void NotificarPorCorreo(string destinatario, string asunto, string cuerpoHtml)
    {
        try
        {
            _emailSender.EnviarCorreoAsync(destinatario, asunto, cuerpoHtml)
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo notificar por correo a {Destinatario}", destinatario);
        }
    }
}