using AutoMarket.API.Controllers;
using AutoMarket.Application.DTOs.Anuncio;
using AutoMarket.Application.Features.Comparador.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AutoMarket.Tests.Controllers;

public class ComparadorControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly ComparadorController _controller;

    public ComparadorControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new ComparadorController(_mockMediator.Object);
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

        _mockMediator
            .Setup(s => s.Send(It.IsAny<CompararVehiculosQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultadoEsperado);

        // Act
        var resultado = await _controller.CompararVehiculos(ids);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        var value = Assert.IsAssignableFrom<IEnumerable<AnuncioComparadorDto>>(ok.Value);
        Assert.Equal(2, value.Count());

        _mockMediator.Verify(s => s.Send(It.IsAny<CompararVehiculosQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompararVehiculos_DatosInvalidos_DebeRetornarBadRequest()
    {
        // Arrange
        var ids = new[] { 1 };

        _mockMediator
            .Setup(s => s.Send(It.IsAny<CompararVehiculosQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Debes seleccionar entre 2 y 4 vehículos para comparar."));

        // Act
        var resultado = await _controller.CompararVehiculos(ids);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        var mensaje = badRequest.Value?.GetType().GetProperty("mensaje")?.GetValue(badRequest.Value)?.ToString();

        Assert.Equal("Debes seleccionar entre 2 y 4 vehículos para comparar.", mensaje);
        _mockMediator.Verify(s => s.Send(It.IsAny<CompararVehiculosQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompararVehiculos_VehiculosNoEncontrados_DebeRetornarNotFound()
    {
        // Arrange
        var ids = new[] { 10, 20 };

        _mockMediator
            .Setup(s => s.Send(It.IsAny<CompararVehiculosQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("No se encontraron los vehículos solicitados."));

        // Act
        var resultado = await _controller.CompararVehiculos(ids);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(resultado);
        var mensaje = notFound.Value?.GetType().GetProperty("mensaje")?.GetValue(notFound.Value)?.ToString();

        Assert.Equal("No se encontraron los vehículos solicitados.", mensaje);
        _mockMediator.Verify(s => s.Send(It.IsAny<CompararVehiculosQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
