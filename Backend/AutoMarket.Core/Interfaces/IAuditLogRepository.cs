using AutoMarket.Core.Entities;

namespace AutoMarket.Core.Interfaces;

public interface IAuditLogRepository
{
    Task<(IEnumerable<AuditLog> Items, int Total)> ObtenerPaginadosAsync(
        int pagina, int tamanoPagina, string? entidad = null, int? usuarioId = null);
}
