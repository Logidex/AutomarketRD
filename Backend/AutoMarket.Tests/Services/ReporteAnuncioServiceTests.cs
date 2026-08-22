using AutoMarket.Application.DTOs.Reportes;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.Services;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;
using AutoMarket.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AutoMarket.Tests.Services;

public class ReporteAnuncioServiceTests
{
    private readonly Mock<IReporteAnuncioRepository> _reportes = new();
    private readonly Mock<IAnuncioRepository> _anuncios = new();

    private ReporteAnuncioService CrearServicio()
    {
        return new ReporteAnuncioService(
            _reportes.Object,
            _anuncios.Object,
            new Mock<IAlmacenadorArchivos>().Object,
            Mock.Of<ILogger<ReporteAnuncioService>>());
    }

    private static Anuncio CrearAnuncioPublicado()
    {
        var anuncio = new Anuncio(
            usuarioId: 1,
            marca: "Toyota", modelo: "Corolla", version: "LE",
            tipoVehiculo: "Sedan", motor: "1.8L", traccion: "FWD",
            colorExterior: "Blanco", colorInterior: "Gris",
            anio: 2020, precio: 950000m, moneda: "DOP",
            kilometraje: 50000, transmision: "Automatica", combustible: "Gasolina",
            accesorios: new List<string>(), ubicacion: "SD", descripcion: "Bueno");

        typeof(Anuncio).GetProperty("Id")?.SetValue(anuncio, 5);
        typeof(Anuncio).GetProperty("Estado")?.SetValue(anuncio, "Publicado");
        return anuncio;
    }

    [Fact]
    public async Task CrearAsync_AnuncioPublicado_GuardaReportePendiente()
    {
        var anuncio = CrearAnuncioPublicado();
        _anuncios.Setup(r => r.ObtenerPorIdAsync(5)).ReturnsAsync(anuncio);

        var dto = new CrearReporteDto
        {
            AnuncioId = 5,
            Motivo = ReporteMotivo.ContenidoInapropiado,
            Detalle = "Fotos inadecuadas"
        };

        var id = await CrearServicio().CrearAsync(dto, "10.0.0.1");

        Assert.Equal(0, id); // Id lo asigna la BD; el repo aún no lo generó
        _reportes.Verify(r => r.AgregarAsync(It.IsAny<ReporteAnuncio>()), Times.Once);
        _reportes.Verify(r => r.GuardarCambiosAsync(), Times.Once);
    }

    [Fact]
    public async Task CrearAsync_AnuncioInexistente_LanzaBusinessRule()
    {
        _anuncios.Setup(r => r.ObtenerPorIdAsync(99)).ReturnsAsync((Anuncio?)null);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => CrearServicio().CrearAsync(
                new CrearReporteDto { AnuncioId = 99, Motivo = ReporteMotivo.Otro }, "1.1.1.1"));
    }

    [Fact]
    public async Task CrearAsync_AnuncioNoPublicado_LanzaBusinessRule()
    {
        var anuncio = CrearAnuncioPublicado();
        typeof(Anuncio).GetProperty("Estado")?.SetValue(anuncio, "Borrador");
        _anuncios.Setup(r => r.ObtenerPorIdAsync(5)).ReturnsAsync(anuncio);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => CrearServicio().CrearAsync(
                new CrearReporteDto { AnuncioId = 5, Motivo = ReporteMotivo.FraudeEstafa }, "1.1.1.1"));
    }

    [Fact]
    public async Task ResolverEliminando_EliminaAnuncioConFotosYCierraReporte()
    {
        var anuncio = CrearAnuncioPublicado();
        typeof(Anuncio).GetProperty("Id")?.SetValue(anuncio, 5);
        anuncio.AgregarFotos(new List<string> { "uploads/a.jpg", "uploads/b.jpg" });

        var reporte = new ReporteAnuncio(5, ReporteMotivo.ContenidoInapropiado, null, "1.1.1.1");
        typeof(ReporteAnuncio).GetProperty("Id")?.SetValue(reporte, 3);
        typeof(ReporteAnuncio).GetProperty("AnuncioId")?.SetValue(reporte, 5);

        _reportes.Setup(r => r.ObtenerPorIdAsync(3)).ReturnsAsync(reporte);
        _anuncios.Setup(r => r.ObtenerPorIdAsync(5)).ReturnsAsync(anuncio);

        var mockS3 = new Mock<IAlmacenadorArchivos>();

        var servicio = new ReporteAnuncioService(
            _reportes.Object, _anuncios.Object, mockS3.Object,
            Mock.Of<ILogger<ReporteAnuncioService>>());

        await servicio.ResolverEliminandoAnuncioAsync(3, adminId: 2);

        // Fotos eliminadas del bucket
        mockS3.Verify(s => s.EliminarArchivoAsync("uploads/a.jpg"), Times.Once);
        mockS3.Verify(s => s.EliminarArchivoAsync("uploads/b.jpg"), Times.Once);

        // Anuncio y reporte persistidos
        _anuncios.Verify(r => r.Eliminar(anuncio), Times.Once);
        _anuncios.Verify(r => r.GuardarCambiosAsync(), Times.Once);
        _reportes.Verify(r => r.GuardarCambiosAsync(), Times.Once);

        Assert.Equal(ReporteEstado.Resuelto, reporte.Estado);
        Assert.Equal(2, reporte.ResueltoPorAdminId);
    }

    [Fact]
    public async Task ResolverEliminando_YaGestionado_LanzaBusinessRule()
    {
        var reporte = new ReporteAnuncio(5, ReporteMotivo.Otro, null, "1.1.1.1");
        typeof(ReporteAnuncio).GetProperty("Id")?.SetValue(reporte, 3);
        reporte.Descartar(adminId: 1);

        _reportes.Setup(r => r.ObtenerPorIdAsync(3)).ReturnsAsync(reporte);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => CrearServicio().ResolverEliminandoAnuncioAsync(3, adminId: 2));
    }
}
