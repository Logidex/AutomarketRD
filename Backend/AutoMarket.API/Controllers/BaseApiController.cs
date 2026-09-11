using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

/// <summary>
/// Controller base con soporte para MediatR (CQRS).
/// Los controllers heredan de aquí y usan IMediator en lugar de I*Service directamente.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected IMediator Mediator { get; }

    protected BaseApiController(IMediator mediator)
    {
        Mediator = mediator;
    }

    /// <summary>
    /// Obtiene el ID del usuario autenticado desde los claims JWT.
    /// </summary>
    protected int? ObtenerUsuarioId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : null;
    }

    /// <summary>
    /// Obtiene el ID del usuario autenticado (requiere autenticación).
    /// Lanza Unauthorized si no hay usuario.
    /// </summary>
    protected int ObtenerUsuarioIdRequerido()
    {
        var id = ObtenerUsuarioId();
        if (id == null)
            throw new UnauthorizedAccessException("Usuario no autenticado.");
        return id.Value;
    }
}
