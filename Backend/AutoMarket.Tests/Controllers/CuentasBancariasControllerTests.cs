using AutoMarket.API.Controllers;
using AutoMarket.Application.Features.CuentasBancarias.Queries;
using AutoMarket.Core.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AutoMarket.Tests.Controllers;

public class CuentasBancariasControllerTests
{
    [Fact]
    public async Task ObtenerCuentasActivas_DebeRetornarOkConLista()
    {
        var mockMediator = new Mock<IMediator>();
        mockMediator.Setup(s => s.Send(It.IsAny<ObtenerCuentasActivasQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CuentaBancaria>());

        var controller = new CuentasBancariasController(mockMediator.Object);

        var resultado = await controller.ObtenerCuentasActivas();

        Assert.IsType<OkObjectResult>(resultado);
        mockMediator.Verify(s => s.Send(It.IsAny<ObtenerCuentasActivasQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
