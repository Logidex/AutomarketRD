using System.Collections.Generic;
using System.Linq;

namespace AutoMarket.Application.Helpers;

public static class AnuncioFotos
{
    public static string? ObtenerPrincipal(IEnumerable<string>? fotos)
    {
        return fotos != null && fotos.Any() ? fotos.First() : null;
    }
}