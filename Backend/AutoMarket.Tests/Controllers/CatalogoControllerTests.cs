using AutoMarket.API.Controllers;
using AutoMarket.Application.DTOs.Anuncio;
using AutoMarket.Application.DTOs.Common;
using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AutoMarket.Tests.Controllers;

public class CatalogoControllerTests
{
    private readonly Mock<ICatalogoService> _mockCatalogoService;
    private readonly CatalogoController _controller;

    public CatalogoControllerTests()
    {
        _mockCatalogoService = new Mock<ICatalogoService>();
        _controller = new CatalogoController(_mockCatalogoService.Object);
    }

    [Fact]
    public async Task ObtenerAnuncios_DebeRetornarOkConResultadoDelServicio()
    {
        // Arrange
        var items = new List<AnuncioCatalogoDto>
        {
            new AnuncioCatalogoDto
            {
                Id = 1,
                Marca = "Toyota",
                Modelo = "Corolla",
                Anio = 2022,
                Precio = 18500m,
                Kilometraje = 25000,
                FotoPrincipal = "foto1.jpg"
            },
            new AnuncioCatalogoDto
            {
                Id = 2,
                Marca = "Honda",
                Modelo = "Civic",
                Anio = 2021,
                Precio = 17500m,
                Kilometraje = 30000,
                FotoPrincipal = "foto2.jpg"
            }
        };

        var resultadoServicio = new PagedResult<AnuncioCatalogoDto>(items, 2, 1, 20);

        _mockCatalogoService
            .Setup(s => s.ObtenerCatalogoPaginadoAsync(1, 20))
            .ReturnsAsync(resultadoServicio);

        // Act
        var resultado = await _controller.ObtenerAnuncios(1, 20);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.Same(resultadoServicio, ok.Value);

        _mockCatalogoService.Verify(s => s.ObtenerCatalogoPaginadoAsync(1, 20), Times.Once);
    }

    [Fact]
    public async Task ObtenerAnuncios_ConParametrosPersonalizados_DebeLlamarServicioConEsosValores()
    {
        // Arrange
        var resultadoServicio = new PagedResult<AnuncioCatalogoDto>(
            new List<AnuncioCatalogoDto>(),
            0,
            3,
            12);

        _mockCatalogoService
            .Setup(s => s.ObtenerCatalogoPaginadoAsync(3, 12))
            .ReturnsAsync(resultadoServicio);

        // Act
        var resultado = await _controller.ObtenerAnuncios(3, 12);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.Same(resultadoServicio, ok.Value);

        _mockCatalogoService.Verify(s => s.ObtenerCatalogoPaginadoAsync(3, 12), Times.Once);
    }

    [Fact]
    public async Task ObtenerAnuncios_SinCambiosEnRespuesta_DebeRetornarStatus200()
    {
        // Arrange
        var resultadoServicio = new PagedResult<AnuncioCatalogoDto>(
            new List<AnuncioCatalogoDto>(),
            0,
            1,
            20);

        _mockCatalogoService
            .Setup(s => s.ObtenerCatalogoPaginadoAsync(1, 20))
            .ReturnsAsync(resultadoServicio);

        // Act
        var resultado = await _controller.ObtenerAnuncios(1, 20);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(200, ok.StatusCode ?? 200);
    }
}