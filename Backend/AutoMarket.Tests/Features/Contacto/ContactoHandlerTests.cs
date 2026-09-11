using Xunit;
using Moq;
using AutoMarket.Application.Features.Contacto.Handlers;
using AutoMarket.Application.Features.Contacto.Commands;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.DTOs;

namespace AutoMarket.Tests.Features.Contacto;

public class ContactoHandlerTests
{
    private readonly Mock<IContactoService> _mockService;
    private readonly ContactoCommandHandler _commandHandler;

    public ContactoHandlerTests()
    {
        _mockService = new Mock<IContactoService>();
        _commandHandler = new ContactoCommandHandler(_mockService.Object);
    }

    [Fact]
    public async Task Handle_ProcesarMensajeContacto_Exitoso_DebeRetornarTrue()
    {
        var dto = new ContactoCreateDto
        {
            Nombre = "Juan Perez",
            Email = "juan@test.com",
            Asunto = "Consulta sobre vehículo",
            Mensaje = "Hola, me interesa este vehículo. ¿Está disponible?"
        };
        var command = new ProcesarMensajeContactoCommand(Dto: dto);
        _mockService
            .Setup(s => s.ProcesarMensajeContactoAsync(dto))
            .ReturnsAsync(true);

        var resultado = await _commandHandler.Handle(command, CancellationToken.None);

        Assert.True(resultado);
        _mockService.Verify(s => s.ProcesarMensajeContactoAsync(dto), Times.Once);
    }

    [Fact]
    public async Task Handle_ProcesarMensajeContacto_Fallido_DebeRetornarFalse()
    {
        var dto = new ContactoCreateDto
        {
            Nombre = "Juan Perez",
            Email = "juan@test.com",
            Asunto = "Consulta",
            Mensaje = "Mensaje"
        };
        var command = new ProcesarMensajeContactoCommand(Dto: dto);
        _mockService
            .Setup(s => s.ProcesarMensajeContactoAsync(dto))
            .ReturnsAsync(false);

        var resultado = await _commandHandler.Handle(command, CancellationToken.None);

        Assert.False(resultado);
        _mockService.Verify(s => s.ProcesarMensajeContactoAsync(dto), Times.Once);
    }
}
