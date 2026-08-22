using AutoMarket.Application.DTOs.Reportes;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;
using AutoMarket.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AutoMarket.Application.Services;

/// <summary>
/// Gestiona los reportes de anuncios: recepción pública, listado para el
/// panel admin y resolución (descartar o eliminar el anuncio reportado).
/// </summary>
public class ReporteAnuncioService : IReporteAnuncioService
{
    private readonly IReporteAnuncioRepository _reportes;
    private readonly IAnuncioRepository _anuncioRepository;
    private readonly IAlmacenadorArchivos _almacenadorArchivos;
    private readonly ILogger<ReporteAnuncioService> _logger;

    public ReporteAnuncioService(
        IReporteAnuncioRepository reportes,
        IAnuncioRepository anuncioRepository,
        IAlmacenadorArchivos almacenadorArchivos,
        ILogger<ReporteAnuncioService> logger)
    {
        _reportes = reportes;
        _anuncioRepository = anuncioRepository;
        _almacenadorArchivos = almacenadorArchivos;
        _logger = logger;
    }

    public async Task<int> CrearAsync(CrearReporteDto dto, string ipReportante)
    {
        var anuncio = await _anuncioRepository.ObtenerPorIdAsync(dto.AnuncioId);

        if (anuncio == null || anuncio.Estado != "Publicado")
            throw new BusinessRuleException("El anuncio reportado no existe o no está publicado.");

        var reporte = new ReporteAnuncio(
            dto.AnuncioId,
            dto.Motivo,
            dto.Detalle,
            ipReportante);

        await _reportes.AgregarAsync(reporte);
        await _reportes.GuardarCambiosAsync();

        _logger.LogInformation(
            "Nuevo reporte {ReporteId} para anuncio {AnuncioId} (motivo: {Motivo}).",
            reporte.Id, dto.AnuncioId, dto.Motivo);

        return reporte.Id;
    }

    public async Task<IReadOnlyCollection<ReporteAdminDto>> ListarPorEstadoAsync(ReporteEstado estado)
    {
        var reportes = await _reportes.ListarPorEstadoAsync(estado);

        return reportes.Select(r => new ReporteAdminDto
        {
            Id = r.Id,
            Motivo = r.Motivo,
            Detalle = r.Detalle,
            Estado = r.Estado,
            FechaCreacionUtc = r.FechaCreacionUtc,
            IpReportante = r.IpReportante,
            AnuncioId = r.Anuncio.Id,
            AnuncioTitulo = r.Anuncio.NombreAnuncio,
            AnuncioEstado = r.Anuncio.Estado,
            AnuncioFotoPrincipal = r.Anuncio.Fotos.FirstOrDefault(),
            AnuncioPrecio = r.Anuncio.Precio,
            AnuncioMoneda = r.Anuncio.Moneda
        }).ToList();
    }

    public Task<int> ContarPendientesAsync()
    {
        return _reportes.ContarPendientesAsync();
    }

    public async Task DescartarAsync(int reporteId, int adminId)
    {
        var reporte = await ObtenerPendienteAsync(reporteId);

        reporte.Descartar(adminId);
        await _reportes.GuardarCambiosAsync();

        _logger.LogInformation("Reporte {ReporteId} descartado por admin {AdminId}.", reporteId, adminId);
    }

    public async Task ResolverEliminandoAnuncioAsync(int reporteId, int adminId)
    {
        var reporte = await ObtenerPendienteAsync(reporteId);

        var anuncio = await _anuncioRepository.ObtenerPorIdAsync(reporte.AnuncioId);

        if (anuncio == null)
            throw new BusinessRuleException("El anuncio reportado ya no existe.");

        if (anuncio.Fotos != null && anuncio.Fotos.Any())
        {
            foreach (var foto in anuncio.Fotos)
            {
                await _almacenadorArchivos.EliminarArchivoAsync(foto);
            }
        }

        _anuncioRepository.Eliminar(anuncio);
        await _anuncioRepository.GuardarCambiosAsync();

        reporte.Resolver(adminId);
        await _reportes.GuardarCambiosAsync();

        _logger.LogWarning(
            "Reporte {ReporteId} RESUELTO eliminando el anuncio {AnuncioId} por admin {AdminId}.",
            reporteId, anuncio.Id, adminId);
    }

    private async Task<ReporteAnuncio> ObtenerPendienteAsync(int reporteId)
    {
        var reporte = await _reportes.ObtenerPorIdAsync(reporteId);

        if (reporte == null)
            throw new BusinessRuleException("El reporte no existe.");

        if (reporte.Estado != ReporteEstado.Pendiente)
            throw new BusinessRuleException("Este reporte ya fue gestionado.");

        return reporte;
    }
}
