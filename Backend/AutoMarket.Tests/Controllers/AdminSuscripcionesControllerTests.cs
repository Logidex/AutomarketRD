using AutoMarket.API.Controllers;
using AutoMarket.Application.DTOs.Admin;
using AutoMarket.Application.Features.Suscripciones.Commands;
using AutoMarket.Application.Features.Suscripciones.Queries;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AutoMarket.Tests.Controllers;

public class AdminSuscripcionesControllerTests
{
    [Fact]
    public async Task ListarPagos_DebeRetornarLaListaDelServicio()
    {
        var pagos = new List<PagoAdminDto>
        {
            new()
            {
                Id = 7,
                PerfilDealerId = 15,
                Nivel = PlanNivel.Pro,
                Ciclo = CicloFacturacion.Mensual,
                Monto = 30m,
                Moneda = "USD"
            }
        };

        var mockMediator = new Mock<IMediator>();
        mockMediator.Setup(s => s.Send(It.IsAny<ObtenerPagosAdminQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagos);

        var controller = new AdminSuscripcionesController(mockMediator.Object);

        var resultado = await controller.ListarPagos();

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var dto = Assert.Single(Assert.IsAssignableFrom<List<PagoAdminDto>>(okResult.Value));
        Assert.Equal(15, dto.PerfilDealerId);
        mockMediator.Verify(s => s.Send(It.IsAny<ObtenerPagosAdminQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReembolsarPago_Exitoso_DebeRetornarOk()
    {
        var mockMediator = new Mock<IMediator>();
        mockMediator.Setup(s => s.Send(It.IsAny<ReembolsarPagoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MetodoPago.PayPal);

        var controller = new AdminSuscripcionesController(mockMediator.Object);

        var resultado = await controller.ReembolsarPago(7);

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.NotNull(okResult);
        mockMediator.Verify(s => s.Send(It.IsAny<ReembolsarPagoCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReembolsarPago_PagoNoEncontrado_DebeRetornarNotFound()
    {
        var mockMediator = new Mock<IMediator>();
        mockMediator
            .Setup(s => s.Send(It.IsAny<ReembolsarPagoCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("No se encontró el pago solicitado."));

        var controller = new AdminSuscripcionesController(mockMediator.Object);

        var resultado = await controller.ReembolsarPago(99);

        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    [Fact]
    public async Task ReembolsarPago_ReglaDeNegocioViolada_DebeRetornarBadRequest()
    {
        var mockMediator = new Mock<IMediator>();
        mockMediator
            .Setup(s => s.Send(It.IsAny<ReembolsarPagoCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException("El pago ya se encuentra reembolsado."));

        var controller = new AdminSuscripcionesController(mockMediator.Object);

        var resultado = await controller.ReembolsarPago(7);

        Assert.IsType<BadRequestObjectResult>(resultado);
    }
}
