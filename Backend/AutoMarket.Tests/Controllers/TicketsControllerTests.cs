using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using AutoMarket.Application.DTOs.Ticket;
using AutoMarket.API.Controllers;
using AutoMarket.Application.Features.Tickets.Commands;
using AutoMarket.Application.Features.Tickets.Queries;
using MediatR;

namespace AutoMarket.Tests.Controllers;

public class TicketsControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly TicketsController _controller;

    public TicketsControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new TicketsController(_mockMediator.Object);
    }

    private void SimularUsuarioAutenticado(string usuarioId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task CrearTicket_DatosValidos_DebeRetornarCreatedConTicketId()
    {
        SimularUsuarioAutenticado("5");

        var dto = new TicketCreateDto
        {
            Asunto = "Problema con pago",
            Categoria = AutoMarket.Core.Entities.Enums.TicketCategoria.Facturacion,
            Mensaje = "Mi tarjeta fue cobrada dos veces."
        };

        _mockMediator.Setup(s => s.Send(
            It.Is<CrearTicketCommand>(c => c.Dto == dto && c.UsuarioId == 5),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(42);

        var resultado = await _controller.CrearTicket(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(resultado);
        Assert.Equal(42, createdResult.RouteValues?["id"]);
    }

    [Fact]
    public async Task CrearTicket_DatosInvalidos_DebeRetornarBadRequest()
    {
        SimularUsuarioAutenticado("5");
        var dto = new TicketCreateDto { Asunto = "Titulo" };
        _controller.ModelState.AddModelError("Mensaje", "Debes escribir un mensaje.");

        var resultado = await _controller.CrearTicket(dto);

        Assert.IsType<BadRequestObjectResult>(resultado);
        _mockMediator.Verify(s => s.Send(It.IsAny<CrearTicketCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ObtenerMisTickets_ConTokenValido_DebeRetornarOk()
    {
        SimularUsuarioAutenticado("5");
        var lista = new List<TicketListadoDto>();

        _mockMediator.Setup(s => s.Send(
            It.Is<ObtenerMisTicketsQuery>(q => q.UsuarioId == 5),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(lista);

        var resultado = await _controller.ObtenerMisTickets();

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(lista, okResult.Value);
    }

    [Fact]
    public async Task ObtenerTicket_DuenoValido_DebeRetornarOk()
    {
        SimularUsuarioAutenticado("5");
        var detalle = new TicketDetalleDto { Id = 3, UsuarioId = 5, Asunto = "Ayuda" };

        _mockMediator.Setup(s => s.Send(
            It.Is<ObtenerTicketQuery>(q => q.TicketId == 3 && q.UsuarioId == 5),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(detalle);

        var resultado = await _controller.ObtenerTicket(3);

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(detalle, okResult.Value);
    }

    [Fact]
    public async Task ObtenerTicket_NoExiste_DebeRetornarNotFound()
    {
        SimularUsuarioAutenticado("5");
        _mockMediator.Setup(s => s.Send(
            It.Is<ObtenerTicketQuery>(q => q.TicketId == 99),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("El ticket no existe."));

        var resultado = await _controller.ObtenerTicket(99);

        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    [Fact]
    public async Task ObtenerTicket_NoEsDueno_DebeRetornarForbidden()
    {
        SimularUsuarioAutenticado("5");
        _mockMediator.Setup(s => s.Send(
            It.IsAny<ObtenerTicketQuery>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Acceso denegado"));

        var resultado = await _controller.ObtenerTicket(3);

        var statusResult = Assert.IsType<ObjectResult>(resultado);
        Assert.Equal(StatusCodes.Status403Forbidden, statusResult.StatusCode);
    }

    [Fact]
    public async Task Responder_MensajeValido_DebeRetornarOk()
    {
        SimularUsuarioAutenticado("5");
        var dto = new TicketMensajeCreateDto { Mensaje = "Gracias por la ayuda." };

        var resultado = await _controller.Responder(3, dto);

        Assert.IsType<OkObjectResult>(resultado);
        _mockMediator.Verify(s => s.Send(
            It.Is<AgregarMensajeTicketCommand>(c => c.TicketId == 3 && c.UsuarioId == 5),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Responder_TicketCerrado_DebeRetornarBadRequest()
    {
        SimularUsuarioAutenticado("5");
        var dto = new TicketMensajeCreateDto { Mensaje = "¿Pueden reabrir?" };

        _mockMediator.Setup(s => s.Send(
            It.IsAny<AgregarMensajeTicketCommand>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AutoMarket.Core.Exceptions.BusinessRuleException("Este ticket está cerrado."));

        var resultado = await _controller.Responder(3, dto);

        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public async Task Cerrar_TicketValido_DebeRetornarOk()
    {
        SimularUsuarioAutenticado("5");

        var resultado = await _controller.Cerrar(3);

        Assert.IsType<OkObjectResult>(resultado);
        _mockMediator.Verify(s => s.Send(
            It.Is<CerrarTicketCommand>(c => c.TicketId == 3 && c.UsuarioId == 5),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
