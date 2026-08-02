using AutoMarket.Core.Entities;

namespace AutoMarket.Core.Interfaces;

public interface ILeadRepository
{
    // Para guardar un nuevo contacto generado desde el frontend
    Task AgregarAsync(Lead lead);

    // Para ver el detalle de un mensaje específico
    Task<Lead?> ObtenerPorIdAsync(int id);

    // Para ver todos los interesados en un vehículo en particular
    Task<IReadOnlyCollection<Lead>> ObtenerPorAnuncioIdAsync(int anuncioId);

    // Para el panel del vendedor/dealer: ver todos los leads de todo su inventario
    Task<IReadOnlyCollection<Lead>> ObtenerPorUsuarioIdAsync(int usuarioId);

    // Para las métricas del dashboard (KPIs)
    Task<int> ContarLeadsPorUsuarioAsync(int usuarioId);
}