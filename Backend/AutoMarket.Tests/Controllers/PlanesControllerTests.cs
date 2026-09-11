using AutoMarket.API.Controllers;
using AutoMarket.Application.DTOs.Planes;
using AutoMarket.Application.Features.PlanCatalogo.Queries;
using AutoMarket.Core.Entities.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AutoMarket.Tests.Controllers;

public class PlanesControllerTests
{
    [Fact]
    public async Task ObtenerCatalogo_DebeRetornarOkConPlanes()
    {
        var planes = new List<PlanCatalogoDto>
        {
            new() { Nivel = PlanNivel.Gratis, Nombre = "Gratis", PrecioMensual = 0 },
            new() { Nivel = PlanNivel.Pro, Nombre = "Pro", PrecioMensual = 2500 }
        };

        var mockMediator = new Mock<IMediator>();
        mockMediator.Setup(s => s.Send(It.IsAny<ObtenerCatalogoPublicoQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(planes);

        var controller = new PlanesController(mockMediator.Object);

        var resultado = await controller.ObtenerCatalogo();

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var lista = Assert.IsAssignableFrom<List<PlanCatalogoDto>>(okResult.Value);
        Assert.Equal(2, lista.Count);
        mockMediator.Verify(s => s.Send(It.IsAny<ObtenerCatalogoPublicoQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
