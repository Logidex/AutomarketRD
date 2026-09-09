using AutoMarket.API.Controllers;
using AutoMarket.Application.DTOs;
using AutoMarket.Application.Features.Contacto.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutoMarket.Tests.Controllers;

public class ContactoControllerTests
{
    [Fact]
    public async Task EnviarMensajeContacto_DatosValidos_DebeRetornarOk()
    {
        var mockMediator = new Mock<IMediator>();
        var mockLogger = new Mock<ILogger<ContactoController>>();
        mockMediator.Setup(s => s.Send(It.IsAny<ProcesarMensajeContactoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = new ContactoController(mockMediator.Object, mockLogger.Object);

        var dto = new ContactoCreateDto
        {
            Nombre = "Juan",
            Email = "juan@test.com",
            Mensaje = "Hola, tengo una consulta"
        };

        var resultado = await controller.EnviarMensajeContacto(dto);

        Assert.IsType<OkObjectResult>(resultado);
        mockMediator.Verify(s => s.Send(It.IsAny<ProcesarMensajeContactoCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnviarMensajeContacto_HoneypotDetectado_DebeRetornarOkSinProcesar()
    {
        var mockMediator = new Mock<IMediator>();
        var mockLogger = new Mock<ILogger<ContactoController>>();

        var controller = new ContactoController(mockMediator.Object, mockLogger.Object);

        var dto = new ContactoCreateDto
        {
            Nombre = "Bot",
            Email = "bot@test.com",
            Mensaje = "Spam",
            Website = "http://bot.com"
        };

        var resultado = await controller.EnviarMensajeContacto(dto);

        Assert.IsType<OkObjectResult>(resultado);
        mockMediator.Verify(s => s.Send(It.IsAny<ProcesarMensajeContactoCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
