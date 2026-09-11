using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Auth;
using AutoMarket.Application.DTOs.Usuario;
using MediatR;

namespace AutoMarket.Application.Features.Auth.Commands;

public record RegistrarUsuarioCommand(RegistroDto Registro)
    : IRequest<(bool Exito, string Mensaje)>;

public record LoginCommand(LoginDto Login)
    : IRequest<LoginResultDto>;

public record RefrescarSesionCommand(string RefreshTokenCrudo)
    : IRequest<LoginResultDto>;

public record LogoutCommand(string? RefreshTokenCrudo)
    : IRequest;

public record SolicitarRecuperacionCommand(string Email)
    : IRequest;

public record RestablecerPasswordCommand(RestablecerPasswordDto Datos)
    : IRequest;

public record ConfirmarCorreoCommand(string Token)
    : IRequest;

public record ReenviarConfirmacionCommand(string Email)
    : IRequest;

public record ActualizarDatosCommand(int UsuarioId, ActualizarDatosDto Datos)
    : IRequest<UsuarioCuentaDto>;

public record AscenderRolCommand(int UsuarioId, AscenderRolDto Datos)
    : IRequest<LoginResultDto>;

public record SolicitarCambioPasswordCommand(int UsuarioId, SolicitarCambioPasswordDto Datos)
    : IRequest;

public record ConfirmarCambioPasswordCommand(int UsuarioId, ConfirmarPasswordDto Datos)
    : IRequest;

public record SolicitarCambioEmailCommand(int UsuarioId, SolicitarCambioEmailDto Datos)
    : IRequest;

public record ConfirmarCambioEmailCommand(int UsuarioId, ConfirmarEmailDto Datos)
    : IRequest;
