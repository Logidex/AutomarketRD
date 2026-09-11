using AutoMarket.API.Controllers;
using AutoMarket.Application.DTOs.Reportes;
using AutoMarket.Application.Features.ReportesAnuncio.Commands;
using AutoMarket.Core.Entities.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AutoMarket.Tests.Controllers;

public class ReportesControllerTests
{
    [Fact]
    public async Task Crear_DatosValidos_DebeRetornarOkConId()
    {
        var mockMediator = new Mock<IMediator>();
        mockMediator.Setup(s => s.Send(It.IsAny<CrearReporteCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(42);

        var controller = new ReportesController(mockMediator.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var dto = new CrearReporteDto
        {
            AnuncioId = 1,
            Motivo = ReporteMotivo.ContenidoInapropiado
        };

        var resultado = await controller.Crear(dto);

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        mockMediator.Verify(s => s.Send(It.IsAny<CrearReporteCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
