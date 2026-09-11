using Xunit;
using Moq;
using AutoMarket.Application.Features.Catalogo.Handlers;
using AutoMarket.Application.Features.Catalogo.Queries;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Anuncio;

namespace AutoMarket.Tests.Features.Catalogo;

public class CatalogoHandlerTests
{
    private readonly Mock<ICatalogoService> _mockService;
    private readonly CatalogoQueryHandler _queryHandler;

    public CatalogoHandlerTests()
    {
        _mockService = new Mock<ICatalogoService>();
        _queryHandler = new CatalogoQueryHandler(_mockService.Object);
    }

    [Fact]
    public async Task Handle_ObtenerCatalogoPaginado_DebeRetornarResultado()
    {
        var query = new ObtenerCatalogoPaginadoQuery(Pagina: 1, TamanoPagina: 10);
        var items = new List<AnuncioCatalogoDto>
        {
            new() { Id = 1, Marca = "Toyota", Modelo = "Corolla", Anio = 2023, Precio = 1200000 },
            new() { Id = 2, Marca = "Honda", Modelo = "Civic", Anio = 2022, Precio = 950000 }
        };
        var expected = new PagedResult<AnuncioCatalogoDto>(items, 25, 1, 10);
        _mockService
            .Setup(s => s.ObtenerCatalogoPaginadoAsync(1, 10))
            .ReturnsAsync(expected);

        var resultado = await _queryHandler.Handle(query, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal(25, resultado.TotalRegistros);
        Assert.Equal(2, resultado.Items.Count);
        _mockService.Verify(s => s.ObtenerCatalogoPaginadoAsync(1, 10), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerCatalogoPaginado_PaginaVacia_DebeRetornarVacio()
    {
        var query = new ObtenerCatalogoPaginadoQuery(Pagina: 5, TamanoPagina: 10);
        var expected = new PagedResult<AnuncioCatalogoDto>(
            new List<AnuncioCatalogoDto>(), 25, 5, 10);
        _mockService
            .Setup(s => s.ObtenerCatalogoPaginadoAsync(5, 10))
            .ReturnsAsync(expected);

        var resultado = await _queryHandler.Handle(query, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Empty(resultado.Items);
        Assert.Equal(5, resultado.PaginaActual);
        _mockService.Verify(s => s.ObtenerCatalogoPaginadoAsync(5, 10), Times.Once);
    }
}
