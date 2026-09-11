using Xunit;
using Moq;
using AutoMarket.Application.Features.Comparador.Handlers;
using AutoMarket.Application.Features.Comparador.Queries;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.DTOs.Anuncio;

namespace AutoMarket.Tests.Features.Comparador;

public class ComparadorHandlerTests
{
    private readonly Mock<IComparadorService> _mockService;
    private readonly ComparadorQueryHandler _queryHandler;

    public ComparadorHandlerTests()
    {
        _mockService = new Mock<IComparadorService>();
        _queryHandler = new ComparadorQueryHandler(_mockService.Object);
    }

    [Fact]
    public async Task Handle_CompararVehiculos_DebeRetornarLista()
    {
        var ids = new[] { 1, 2 };
        var query = new CompararVehiculosQuery(ids);
        var expected = new List<AnuncioComparadorDto>
        {
            new() { Id = 1, Marca = "Toyota", Modelo = "Corolla", Anio = 2023, Precio = 1200000 },
            new() { Id = 2, Marca = "Honda", Modelo = "Civic", Anio = 2022, Precio = 950000 }
        };
        _mockService
            .Setup(s => s.CompararVehiculosAsync(ids))
            .ReturnsAsync(expected);

        var resultado = await _queryHandler.Handle(query, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count());
        _mockService.Verify(s => s.CompararVehiculosAsync(ids), Times.Once);
    }

    [Fact]
    public async Task Handle_CompararVehiculos_Vacio_DebeRetornarListaVacia()
    {
        var ids = Array.Empty<int>();
        var query = new CompararVehiculosQuery(ids);
        _mockService
            .Setup(s => s.CompararVehiculosAsync(ids))
            .ReturnsAsync(new List<AnuncioComparadorDto>());

        var resultado = await _queryHandler.Handle(query, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Empty(resultado);
        _mockService.Verify(s => s.CompararVehiculosAsync(ids), Times.Once);
    }

    [Fact]
    public async Task Handle_CompararVehiculos_UnSoloId_DebeRetornarLista()
    {
        var ids = new[] { 5 };
        var query = new CompararVehiculosQuery(ids);
        var expected = new List<AnuncioComparadorDto>
        {
            new() { Id = 5, Marca = "Ford", Modelo = "Explorer", Anio = 2024, Precio = 2500000 }
        };
        _mockService
            .Setup(s => s.CompararVehiculosAsync(ids))
            .ReturnsAsync(expected);

        var resultado = await _queryHandler.Handle(query, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Single(resultado);
        Assert.Equal("Ford", resultado.First().Marca);
        _mockService.Verify(s => s.CompararVehiculosAsync(ids), Times.Once);
    }
}
