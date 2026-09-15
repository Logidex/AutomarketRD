using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Application.Services;

/// <summary>
/// Servicio para crear notificaciones in-app.
/// Otros servicios lo inyectan para notificar eventos.
/// </summary>
public class NotificacionService
{
    private readonly INotificacionRepository _repo;

    public NotificacionService(INotificacionRepository repo)
    {
        _repo = repo;
    }

    public async Task NotificarLeadAsync(int vendedorId, string marca, string modelo, string nombreContacto)
    {
        await _repo.CrearAsync(new Notificacion
        {
            UsuarioId = vendedorId,
            Titulo = "Nuevo lead recibido",
            Mensaje = $"{nombreContacto} está interesado en tu {marca} {modelo}.",
            Tipo = "lead",
            UrlDestino = "/dashboard/leads"
        });
    }

    public async Task NotificarSuscripcionVenciendoAsync(int usuarioId, string nombrePlan, int diasRestantes)
    {
        await _repo.CrearAsync(new Notificacion
        {
            UsuarioId = usuarioId,
            Titulo = "Suscripción proxima a vencer",
            Mensaje = $"Tu plan {nombrePlan} vence en {diasRestantes} días. Renueva para mantener tus anuncios activos.",
            Tipo = "suscripcion",
            UrlDestino = "/dashboard/suscripcion"
        });
    }

    public async Task NotificarTransferenciaAprobadaAsync(int usuarioId, decimal monto, string moneda)
    {
        await _repo.CrearAsync(new Notificacion
        {
            UsuarioId = usuarioId,
            Titulo = "Transferencia aprobada",
            Mensaje = $"Tu pago de {monto:N2} {moneda} fue procesado exitosamente.",
            Tipo = "transferencia",
            UrlDestino = "/dashboard/pagos"
        });
    }
}
