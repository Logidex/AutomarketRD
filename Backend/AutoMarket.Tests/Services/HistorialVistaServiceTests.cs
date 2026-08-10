using AutoMarket.Application.Services;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using Moq;
using Xunit;

namespace AutoMarket.Tests.Services;

public class HistorialVistaServiceTests
{
    private readonly Mock<IHistorialVistaRepository> _mockHistorialRepo;
    private readonly Mock<IAnuncioRepository> _mockAnuncioRepo;
    private readonly HistorialVistaService _service;

    public HistorialVistaServiceTests()
    {
        _mockHistorialRepo = new Mock<IHistorialVistaRepository>();
        _mockAnuncioRepo = new Mock<IAnuncioRepository>();
        _service = new HistorialVistaService(_mockHistorialRepo.Object, _mockAnuncioRepo.Object);
    }

    private static Anuncio CrearAnuncioSimulado(int id, int usuarioId, string marca, string modelo)
    {
        var anuncio = new Anuncio(
            usuarioId,
            marca,
            modelo,
            "",
            "Sedan",
            "1.8L",
            "Delantera",
            "Blanco",
            "Negro",
            2024,
            20000m,
            25000,
            "Automática",
            "Gasolina",
            new List<string>(),
            "Santo Domingo",
            "Vehículo de prueba"
        );

        typeof(Anuncio).GetProperty("Id")?.SetValue(anuncio, id);
        return anuncio;
    }

    private static void PublicarAnuncio(Anuncio anuncio)
    {
        anuncio.AgregarFotos(new List<string> { "f1", "f2", "f3", "f4", "f5" });
        anuncio.Publicar();
    }

    [Fact]
    public async Task RegistrarVistaAsync_AnuncioNoExiste_NoRegistraNada()
    {
        _mockAnuncioRepo
            .Setup(r => r.ObtenerPorIdAsync(1))
            .ReturnsAsync((Anuncio?)null);

        await _service.RegistrarVistaAsync(10, 1);

        _mockHistorialRepo.Verify(r => r.AgregarAsync(It.IsAny<HistorialVista>()), Times.Never);
        _mockHistorialRepo.Verify(r => r.GuardarCambiosAsync(), Times.Never);
    }

    [Fact]
    public async Task RegistrarVistaAsync_AnuncioNoPublicado_NoRegistraNada()
    {
        var anuncio = CrearAnuncioSimulado(1, 5, "Honda", "Civic");

        _mockAnuncioRepo
            .Setup(r => r.ObtenerPorIdAsync(1))
            .ReturnsAsync(anuncio);

        await _service.RegistrarVistaAsync(10, 1);

        _mockHistorialRepo.Verify(r => r.AgregarAsync(It.IsAny<HistorialVista>()), Times.Never);
        _mockHistorialRepo.Verify(r => r.GuardarCambiosAsync(), Times.Never);
    }

    [Fact]
    public async Task RegistrarVistaAsync_NuevaVista_AgregaHistorial()
    {
        var anuncio = CrearAnuncioSimulado(1, 5, "Toyota", "Corolla");
        PublicarAnuncio(anuncio);

        _mockAnuncioRepo
            .Setup(r => r.ObtenerPorIdAsync(1))
            .ReturnsAsync(anuncio);

        _mockHistorialRepo
            .Setup(r => r.ObtenerAsync(10, 1))
            .ReturnsAsync((HistorialVista?)null);

        await _service.RegistrarVistaAsync(10, 1);

        _mockHistorialRepo.Verify(
            r => r.AgregarAsync(It.Is<HistorialVista>(h => h.UsuarioId == 10 && h.AnuncioId == 1)),
            Times.Once);
    }

    [Fact]
    public async Task RegistrarVistaAsync_YaExistia_ActualizaTimestampSinDuplicar()
    {
        var anuncio = CrearAnuncioSimulado(1, 5, "Ford", "Escape");
        PublicarAnuncio(anuncio);

        var existente = new HistorialVista(10, 1);

        _mockAnuncioRepo
            .Setup(r => r.ObtenerPorIdAsync(1))
            .ReturnsAsync(anuncio);

        _mockHistorialRepo
            .Setup(r => r.ObtenerAsync(10, 1))
            .ReturnsAsync(existente);

        await _service.RegistrarVistaAsync(10, 1);

        _mockHistorialRepo.Verify(r => r.AgregarAsync(It.IsAny<HistorialVista>()), Times.Never);
        _mockHistorialRepo.Verify(r => r.GuardarCambiosAsync(), Times.Once);
        Assert.True(DateTime.UtcNow - existente.VistoEnUtc < TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task ObtenerRecientesAsync_CantidadFueraDeRango_UsaDoce()
    {
        _mockHistorialRepo
            .Setup(r => r.ObtenerRecientesAsync(10, 12))
            .ReturnsAsync(new List<HistorialVista>());

        await _service.ObtenerRecientesAsync(10, cantidad: 9999);

        _mockHistorialRepo.Verify(r => r.ObtenerRecientesAsync(10, 12), Times.Once);
    }

    [Fact]
    public async Task ObtenerRecientesAsync_DebeMapearDtosConLaFechaDeVista()
    {
        var anuncio = CrearAnuncioSimulado(1, 5, "Toyota", "Corolla");
        PublicarAnuncio(anuncio);

        var historial = new HistorialVista(10, 1);
        var fechaVista = new DateTime(2026, 8, 1, 12, 0, 0, DateTimeKind.Utc);

        typeof(HistorialVista).GetProperty("Anuncio")?.SetValue(historial, anuncio);
        typeof(HistorialVista).GetProperty("VistoEnUtc")?.SetValue(historial, fechaVista);

        _mockHistorialRepo
            .Setup(r => r.ObtenerRecientesAsync(10, 12))
            .ReturnsAsync(new List<HistorialVista> { historial });

        var resultado = await _service.ObtenerRecientesAsync(10, 12);

        var dto = Assert.Single(resultado);
        Assert.Equal("Toyota", dto.Marca);
        Assert.Equal("Corolla", dto.Modelo);
        Assert.Equal(1, dto.Id);
        Assert.Equal("f1", dto.FotoPrincipal);
        Assert.Equal(fechaVista, dto.VistoEnUtc);
    }
}