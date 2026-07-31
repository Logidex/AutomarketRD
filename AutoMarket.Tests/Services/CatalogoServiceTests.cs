using AutoMarket.Application.DTOs.Anuncio;
using AutoMarket.Application.DTOs.Common;
using AutoMarket.Application.Services;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using Moq;
using Xunit;

namespace AutoMarket.Tests.Services;

public class CatalogoServiceTests
{
    private readonly Mock<IAnuncioRepository> _mockAnuncioRepository;
    private readonly CatalogoService _service;

    public CatalogoServiceTests()
    {
        _mockAnuncioRepository = new Mock<IAnuncioRepository>();
        _service = new CatalogoService(_mockAnuncioRepository.Object);
    }

    private static Anuncio CrearAnuncioSimulado(
        int usuarioId = 1,
        string marca = "Toyota",
        string modelo = "Corolla",
        int anio = 2022,
        decimal precio = 18500m,
        int kilometraje = 25000,
        string transmision = "Automática",
        string combustible = "Gasolina",
        string colorExterior = "Blanco",
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
    public async Task ObtenerCatalogoPaginadoAsync_PaginaMenorAUno_DebeUsarPaginaUno()
    {
        _mockAnuncioRepository
            .Setup(r => r.ObtenerPaginadosAsync(1, 10))
            .ReturnsAsync((new List<Anuncio>(), 0));

        await _service.ObtenerCatalogoPaginadoAsync(0, 10);

        _mockAnuncioRepository.Verify(r => r.ObtenerPaginadosAsync(1, 10), Times.Once);
    }

    [Fact]
    public async Task ObtenerCatalogoPaginadoAsync_TamanoMenorAUno_DebeUsarVeinte()
    {
        _mockAnuncioRepository
            .Setup(r => r.ObtenerPaginadosAsync(2, 20))
            .ReturnsAsync((new List<Anuncio>(), 0));

        await _service.ObtenerCatalogoPaginadoAsync(2, 0);

        _mockAnuncioRepository.Verify(r => r.ObtenerPaginadosAsync(2, 20), Times.Once);
    }

    [Fact]
    public async Task ObtenerCatalogoPaginadoAsync_TamanoMayorACincuenta_DebeUsarVeinte()
    {
        _mockAnuncioRepository
            .Setup(r => r.ObtenerPaginadosAsync(3, 20))
            .ReturnsAsync((new List<Anuncio>(), 0));

        await _service.ObtenerCatalogoPaginadoAsync(3, 100);

        _mockAnuncioRepository.Verify(r => r.ObtenerPaginadosAsync(3, 20), Times.Once);
    }

    [Fact]
    public async Task ObtenerCatalogoPaginadoAsync_DatosValidos_DebeRetornarPagedResultGenerico()
    {
        var anuncios = new List<Anuncio>
        {
            CrearAnuncioSimulado(
                usuarioId: 1,
                marca: "Toyota",
                modelo: "Corolla",
                anio: 2022,
                precio: 18500m,
                kilometraje: 25000,
                fotos: new List<string> { "foto1.jpg", "foto2.jpg" })
        };

        _mockAnuncioRepository
            .Setup(r => r.ObtenerPaginadosAsync(1, 20))
            .ReturnsAsync((anuncios, 1));

        var resultado = await _service.ObtenerCatalogoPaginadoAsync(1, 20);

        Assert.IsType<PagedResult<AnuncioCatalogoDto>>(resultado);
        Assert.Single(resultado.Items);
        Assert.Equal(20, resultado.PageSize);
    }

    [Fact]
    public async Task ObtenerCatalogoPaginadoAsync_DebeMapearPrimerElemento()
    {
        var anuncios = new List<Anuncio>
        {
            CrearAnuncioSimulado(
                usuarioId: 1,
                marca: "Toyota",
                modelo: "Corolla",
                anio: 2022,
                precio: 18500m,
                kilometraje: 25000,
                fotos: new List<string> { "foto1.jpg", "foto2.jpg" }),
            CrearAnuncioSimulado(
                usuarioId: 2,
                marca: "Honda",
                modelo: "Civic",
                anio: 2021,
                precio: 17500m,
                kilometraje: 30000,
                fotos: new List<string>())
        };

        _mockAnuncioRepository
            .Setup(r => r.ObtenerPaginadosAsync(1, 20))
            .ReturnsAsync((anuncios, 2));

        var resultado = await _service.ObtenerCatalogoPaginadoAsync(1, 20);

        var lista = resultado.Items.ToList();

        Assert.Equal(2, lista.Count);

        var primerItem = lista[0];
        Assert.Equal("Toyota", primerItem.Marca);
        Assert.Equal("Corolla", primerItem.Modelo);
        Assert.Equal(2022, primerItem.Anio);
        Assert.Equal(18500m, primerItem.Precio);
        Assert.Equal(25000, primerItem.Kilometraje);
    }

    [Fact]
    public async Task ObtenerCatalogoPaginadoAsync_AnuncioSinFotos_DebeRetornarSinFotoPrincipal()
    {
        var anuncios = new List<Anuncio>
        {
            CrearAnuncioSimulado(
                usuarioId: 5,
                marca: "Mazda",
                modelo: "3",
                anio: 2020,
                precio: 16000m,
                kilometraje: 45000,
                fotos: new List<string>())
        };

        _mockAnuncioRepository
            .Setup(r => r.ObtenerPaginadosAsync(1, 20))
            .ReturnsAsync((anuncios, 1));

        var resultado = await _service.ObtenerCatalogoPaginadoAsync(1, 20);

        var item = Assert.Single(resultado.Items);

        Assert.Equal("Mazda", item.Marca);
        Assert.Equal("3", item.Modelo);
    }
}