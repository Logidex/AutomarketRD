using AutoMarket.API.Controllers;
using AutoMarket.Application.DTOs.Anuncio;
using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AutoMarket.Tests.Controllers;

public class ComparadorControllerTests
{
    private readonly Mock<IComparadorService> _mockComparadorService;
    private readonly ComparadorController _controller;

    public ComparadorControllerTests()
    {
        _mockComparadorService = new Mock<IComparadorService>();
        _controller = new ComparadorController(_mockComparadorService.Object);
    }

    [Fact]
    public async Task CompararVehiculos_DatosValidos_DebeRetornarOkConResultado()
    {
        // Arrange
        var ids = new[] { 1, 2 };

        var resultadoEsperado = new List<AnuncioComparadorDto>
        {
            new AnuncioComparadorDto
            {
                Id = 1,
                Marca = "Toyota",
                Modelo = "Corolla",
                Anio = 2022,
                Precio = 18500m,
                Kilometraje = 25000,
                Transmision = "Automática",
                Combustible = "Gasolina",
                ColorExterior = "Gris",
                FotoPrincipal = "foto1.jpg"
            },
            new AnuncioComparadorDto
            {
                Id = 2,
                Marca = "Honda",
                Modelo = "Civic",
                Anio = 2021,
                Precio = 17500m,
                Kilometraje = 32000,
                Transmision = "Manual",
                Combustible = "Gasolina",
                ColorExterior = "Negro",
                FotoPrincipal = "foto2.jpg"
            }
        };

        _mockComparadorService
            .Setup(s => s.CompararVehiculosAsync(ids))
            .ReturnsAsync(resultadoEsperado);

        // Act
        var resultado = await _controller.CompararVehiculos(ids);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        var value = Assert.IsAssignableFrom<IEnumerable<AnuncioComparadorDto>>(ok.Value);
        Assert.Equal(2, value.Count());

        _mockComparadorService.Verify(s => s.CompararVehiculosAsync(ids), Times.Once);
    }

    [Fact]
    public async Task CompararVehiculos_DatosInvalidos_DebeRetornarBadRequest()
    {
        // Arrange
        var ids = new[] { 1 };

        _mockComparadorService
            .Setup(s => s.CompararVehiculosAsync(ids))
            .ThrowsAsync(new ArgumentException("Debes seleccionar entre 2 y 4 vehículos para comparar."));

        // Act
        var resultado = await _controller.CompararVehiculos(ids);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        var mensaje = badRequest.Value?.GetType().GetProperty("mensaje")?.GetValue(badRequest.Value)?.ToString();

        Assert.Equal("Debes seleccionar entre 2 y 4 vehículos para comparar.", mensaje);
        _mockComparadorService.Verify(s => s.CompararVehiculosAsync(ids), Times.Once);
    }

    [Fact]
    public async Task CompararVehiculos_VehiculosNoEncontrados_DebeRetornarNotFound()
    {
        // Arrange
        var ids = new[] { 10, 20 };

        _mockComparadorService
            .Setup(s => s.CompararVehiculosAsync(ids))
            .ThrowsAsync(new KeyNotFoundException("No se encontraron los vehículos solicitados."));

        // Act
        var resultado = await _controller.CompararVehiculos(ids);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(resultado);
        var mensaje = notFound.Value?.GetType().GetProperty("mensaje")?.GetValue(notFound.Value)?.ToString();

        Assert.Equal("No se encontraron los vehículos solicitados.", mensaje);
        _mockComparadorService.Verify(s => s.CompararVehiculosAsync(ids), Times.Once);
    }
}