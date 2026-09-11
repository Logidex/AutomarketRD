using AutoMarket.Application.DTOs.Auth;
using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Application.Features.Auth.Commands;
using AutoMarket.Application.Features.Auth.Queries;
using AutoMarket.Application.Interfaces;
using MediatR;

namespace AutoMarket.Application.Features.Auth.Handlers;

public class AuthCommandHandler
    : IRequestHandler<RegistrarUsuarioCommand, (bool Exito, string Mensaje)>,
      IRequestHandler<LoginCommand, LoginResultDto>,
      IRequestHandler<RefrescarSesionCommand, LoginResultDto>,
      IRequestHandler<LogoutCommand>,
      IRequestHandler<SolicitarRecuperacionCommand>,
      IRequestHandler<RestablecerPasswordCommand>,
      IRequestHandler<ConfirmarCorreoCommand>,
      IRequestHandler<ReenviarConfirmacionCommand>,
      IRequestHandler<ActualizarDatosCommand, UsuarioCuentaDto>,
      IRequestHandler<AscenderRolCommand, LoginResultDto>,
      IRequestHandler<SolicitarCambioPasswordCommand>,
      IRequestHandler<ConfirmarCambioPasswordCommand>,
      IRequestHandler<SolicitarCambioEmailCommand>,
      IRequestHandler<ConfirmarCambioEmailCommand>
{
    private readonly IAuthService _authService;
    private readonly IUsuarioCuentaService _cuentaService;

    public AuthCommandHandler(IAuthService authService, IUsuarioCuentaService cuentaService)
    {
        _authService = authService;
        _cuentaService = cuentaService;
    }

    public async Task<(bool Exito, string Mensaje)> Handle(
        RegistrarUsuarioCommand request, CancellationToken ct)
    {
        return await _authService.RegistrarUsuarioAsync(request.Registro);
    }

    public async Task<LoginResultDto> Handle(
        LoginCommand request, CancellationToken ct)
    {
        return await _authService.LoginAsync(request.Login);
    }

    public async Task<LoginResultDto> Handle(
        RefrescarSesionCommand request, CancellationToken ct)
    {
        return await _authService.RefrescarSesionAsync(request.RefreshTokenCrudo);
    }

    public async Task Handle(
        LogoutCommand request, CancellationToken ct)
    {
        await _authService.RevocarSesionAsync(request.RefreshTokenCrudo);
    }

    public async Task Handle(
        SolicitarRecuperacionCommand request, CancellationToken ct)
    {
        await _authService.SolicitarRecuperacionAsync(request.Email);
    }

    public async Task Handle(
        RestablecerPasswordCommand request, CancellationToken ct)
    {
        await _authService.RestablecerPasswordAsync(request.Datos);
    }

    public async Task Handle(
        ConfirmarCorreoCommand request, CancellationToken ct)
    {
        await _authService.ConfirmarCorreoAsync(request.Token);
    }

    public async Task Handle(
        ReenviarConfirmacionCommand request, CancellationToken ct)
    {
        await _authService.ReenviarConfirmacionCorreoAsync(request.Email);
    }

    public async Task<UsuarioCuentaDto> Handle(
        ActualizarDatosCommand request, CancellationToken ct)
    {
        return await _cuentaService.ActualizarDatosAsync(request.UsuarioId, request.Datos);
    }

    public async Task<LoginResultDto> Handle(
        AscenderRolCommand request, CancellationToken ct)
    {
        return await _cuentaService.AscenderRolAsync(request.UsuarioId, request.Datos);
    }

    public async Task Handle(
        SolicitarCambioPasswordCommand request, CancellationToken ct)
    {
        await _cuentaService.SolicitarCambioPasswordAsync(request.UsuarioId, request.Datos);
    }

    public async Task Handle(
        ConfirmarCambioPasswordCommand request, CancellationToken ct)
    {
        await _cuentaService.ConfirmarCambioPasswordAsync(request.UsuarioId, request.Datos);
    }

    public async Task Handle(
        SolicitarCambioEmailCommand request, CancellationToken ct)
    {
        await _cuentaService.SolicitarCambioEmailAsync(request.UsuarioId, request.Datos);
    }

    public async Task Handle(
        ConfirmarCambioEmailCommand request, CancellationToken ct)
    {
        await _cuentaService.ConfirmarCambioEmailAsync(request.UsuarioId, request.Datos);
    }
}

public class AuthQueryHandler
    : IRequestHandler<ObtenerCuentaQuery, UsuarioCuentaDto>
{
    private readonly IUsuarioCuentaService _cuentaService;

    public AuthQueryHandler(IUsuarioCuentaService cuentaService)
    {
        _cuentaService = cuentaService;
    }

    public async Task<UsuarioCuentaDto> Handle(
        ObtenerCuentaQuery request, CancellationToken ct)
    {
        return await _cuentaService.ObtenerCuentaAsync(request.UsuarioId);
    }
}
