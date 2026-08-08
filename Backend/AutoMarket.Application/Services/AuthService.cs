using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Auth;
using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Interfaces;
using BCrypt.Net;

namespace AutoMarket.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _repository;
    private readonly ITokenService _tokenService;
    private readonly ISuscripcionService _suscripcionService;

    public AuthService(
        IUsuarioRepository repository,
        ITokenService tokenService,
        ISuscripcionService suscripcionService)
    {
        _repository = repository;
        _tokenService = tokenService;
        _suscripcionService = suscripcionService;
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

            var planInicial = string.IsNullOrWhiteSpace(dto.PlanInicial)
                ? "Gratis"
                : dto.PlanInicial.Trim();

            if (!EsPlanGratis(planInicial))
            {
                return (false, "Los planes de pago estarán disponibles próximamente. Por ahora regístrate con el Plan Gratis.");
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

    private static bool EsPlanGratis(string planInicial)
    {
        return string.Equals(planInicial, "Gratis", StringComparison.OrdinalIgnoreCase);
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
}

