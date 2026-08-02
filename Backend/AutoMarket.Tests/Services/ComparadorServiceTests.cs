using AutoMarket.Application.Services;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using Moq;
using Xunit;

namespace AutoMarket.Tests.Services;

public class ComparadorServiceTests
{
    private readonly Mock<IAnuncioRepository> _mockAnuncioRepository;
    private readonly ComparadorService _service;

    public ComparadorServiceTests()
    {
        _mockAnuncioRepository = new Mock<IAnuncioRepository>();
        _service = new ComparadorService(_mockAnuncioRepository.Object);
    }

    private static Anuncio CrearAnuncioSimulado(
        int usuarioId,
        string marca,
        string modelo,
        int anio,
        decimal precio,
        int kilometraje,
        string transmision,
        string combustible,
        string colorExterior,
        List<string>? fotos = null)
    {
        return new Anuncio(
            usuarioId,
            marca,
            modelo,
            "Sedan",
            colorExterior,
            "Negro",
            anio,
            precio,
            kilometraje,
            transmision,
            combustible,
            fotos ?? new List<string>(),
            "Santo Domingo",
            "Vehículo de prueba"
        );
    }

    [Fact]
    public async Task CompararVehiculosAsync_IdsNull_DebeLanzarArgumentException()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CompararVehiculosAsync(null!));

        Assert.Equal("Debes seleccionar entre 2 y 4 vehículos para comparar.", ex.Message);
    }

    [Fact]
    public async Task CompararVehiculosAsync_MenosDeDosIds_DebeLanzarArgumentException()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CompararVehiculosAsync(new[] { 1 }));

        Assert.Equal("Debes seleccionar entre 2 y 4 vehículos para comparar.", ex.Message);
    }

    [Fact]
    public async Task CompararVehiculosAsync_MasDeCuatroIds_DebeLanzarArgumentException()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CompararVehiculosAsync(new[] { 1, 2, 3, 4, 5 }));

        Assert.Equal("Debes seleccionar entre 2 y 4 vehículos para comparar.", ex.Message);
    }

    [Fact]
    public async Task CompararVehiculosAsync_IdsDuplicados_DebeConsultarSoloIdsUnicos()
    {
        _mockAnuncioRepository
            .Setup(r => r.ObtenerPorIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<Anuncio>
            {
                CrearAnuncioSimulado(1, "Toyota", "Corolla", 2020, 15000m, 50000, "Automática", "Gasolina", "Blanco"),
                CrearAnuncioSimulado(2, "Honda", "Civic", 2021, 17000m, 30000, "Automática", "Gasolina", "Negro")
            });

        await _service.CompararVehiculosAsync(new[] { 1, 1, 2 });

        _mockAnuncioRepository.Verify(r =>
            r.ObtenerPorIdsAsync(It.Is<IEnumerable<int>>(ids =>
                ids.Count() == 2 &&
                ids.Contains(1) &&
                ids.Contains(2))), Times.Once);
    }

    [Fact]
    public async Task CompararVehiculosAsync_SinResultados_DebeLanzarKeyNotFoundException()
    {
        _mockAnuncioRepository
            .Setup(r => r.ObtenerPorIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<Anuncio>());

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.CompararVehiculosAsync(new[] { 1, 2 }));

        Assert.Equal("No se encontraron los vehículos solicitados.", ex.Message);
    }

    [Fact]
    public async Task CompararVehiculosAsync_DatosValidos_DebeMapearCorrectamenteYTomarPrimeraFoto()
    {
        _mockAnuncioRepository
            .Setup(r => r.ObtenerPorIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<Anuncio>
            {
            CrearAnuncioSimulado(
                usuarioId: 10,
                marca: "Toyota",
                modelo: "Corolla",
                anio: 2022,
                precio: 18500m,
                kilometraje: 25000,
                transmision: "Automática",
                combustible: "Gasolina",
                colorExterior: "Gris",
                fotos: new List<string> { "foto1.jpg", "foto2.jpg" })
            });

        var resultado = await _service.CompararVehiculosAsync(new[] { 10, 11 });

        var item = Assert.Single(resultado);
        Assert.Equal("Toyota", item.Marca);
        Assert.Equal("Corolla", item.Modelo);
        Assert.Equal(2022, item.Anio);
        Assert.Equal(18500m, item.Precio);
        Assert.Equal(25000, item.Kilometraje);
        Assert.Equal("Automática", item.Transmision);
        Assert.Equal("Gasolina", item.Combustible);
        Assert.Equal("Gris", item.ColorExterior);
    }

    [Fact]
    public async Task CompararVehiculosAsync_AnuncioSinFotos_DebeMapearFotoPrincipalNull()
    {
        _mockAnuncioRepository
            .Setup(r => r.ObtenerPorIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<Anuncio>
            {
                CrearAnuncioSimulado(
                    usuarioId: 20,
                    marca: "Honda",
                    modelo: "Civic",
                    anio: 2021,
                    precio: 17500m,
                    kilometraje: 40000,
                    transmision: "Manual",
                    combustible: "Gasolina",
                    colorExterior: "Azul",
                    fotos: new List<string>())
            });

        var resultado = await _service.CompararVehiculosAsync(new[] { 20, 21 });

        var item = Assert.Single(resultado);
        Assert.Null(item.FotoPrincipal);
    }
}