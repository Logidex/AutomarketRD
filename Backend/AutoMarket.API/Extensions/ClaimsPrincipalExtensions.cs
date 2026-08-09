using System.Security.Claims;

namespace AutoMarket.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int ObtenerUsuarioId(this ClaimsPrincipal user)
    {
        var valor = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(valor) || !int.TryParse(valor, out var usuarioId))
        {
            throw new UnauthorizedAccessException("Token inválido o usuario no identificado.");
        }

        return usuarioId;
    }

    public static int? ObtenerUsuarioIdOpcional(this ClaimsPrincipal user)
    {
        var valor = user.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(valor, out var usuarioId)
            ? usuarioId
            : null;
    }
}