using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using AutoMarket.Application.DTOs.Ticket;
using AutoMarket.Application.Interfaces;
using AutoMarket.API.Controllers;

namespace AutoMarket.Tests.Controllers;

public class TicketsControllerTests
{
    private readonly Mock<ITicketService> _mockTicketService;
    private readonly TicketsController _controller;

    public TicketsControllerTests()
    {
        _mockTicketService = new Mock<ITicketService>();
        _controller = new TicketsController(_mockTicketService.Object);
    }

    // =========================================================================
    // HELPER: Configurar Usuario Autenticado (Simular el Token JWT)
    // =========================================================================
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

    // =========================================================================
    // PRUEBA 01: POST CrearTicket - Éxito (201 Created + mensaje)
    // =========================================================================
    [Fact]
    public async Task CrearTicket_DatosValidos_DebeRetornarCreatedConTicketId()
    {
        // Arrange
        SimularUsuarioAutenticado("5");

        var dto = new TicketCreateDto
        {
            Asunto = "Problema con pago",
            Categoria = AutoMarket.Core.Entities.Enums.TicketCategoria.Facturacion,
            Mensaje = "Mi tarjeta fue cobrada dos veces."
        };

        _mockTicketService.Setup(s => s.CrearTicketAsync(dto, 5)).ReturnsAsync(42);

        // Act
        var resultado = await _controller.CrearTicket(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(resultado);
        Assert.Equal(42, createdResult.RouteValues?["id"]);
        _mockTicketService.Verify(s => s.CrearTicketAsync(dto, 5), Times.Once);
    }

    // =========================================================================
    // PRUEBA 02: POST CrearTicket - Falla por validaciones (ModelState)
    // =========================================================================
    [Fact]
    public async Task CrearTicket_DatosInvalidos_DebeRetornarBadRequest()
    {
        // Arrange
        SimularUsuarioAutenticado("5");
        var dto = new TicketCreateDto { Asunto = "Titulo" }; // Falta Mensaje
        _controller.ModelState.AddModelError("Mensaje", "Debes escribir un mensaje.");

        // Act
        var resultado = await _controller.CrearTicket(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(resultado);
        _mockTicketService.Verify(s => s.CrearTicketAsync(It.IsAny<TicketCreateDto>(), It.IsAny<int>()), Times.Never);
    }

    // =========================================================================
    // PRUEBA 03: GET mis-tickets - Éxito (extrae ID del JWT)
    // =========================================================================
    [Fact]
    public async Task ObtenerMisTickets_ConTokenValido_DebeRetornarOk()
    {
        // Arrange
        SimularUsuarioAutenticado("5");
        var lista = new List<TicketListadoDto>();

        _mockTicketService.Setup(s => s.ObtenerMisTicketsAsync(5)).ReturnsAsync(lista);

        // Act
        var resultado = await _controller.ObtenerMisTickets();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(lista, okResult.Value);
        _mockTicketService.Verify(s => s.ObtenerMisTicketsAsync(5), Times.Once);
    }

    // =========================================================================
    // PRUEBA 04: GET {id} - Éxito (dueño)
    // =========================================================================
    [Fact]
    public async Task ObtenerTicket_DuenoValido_DebeRetornarOk()
    {
        // Arrange
        SimularUsuarioAutenticado("5");
        var detalle = new TicketDetalleDto { Id = 3, UsuarioId = 5, Asunto = "Ayuda" };

        _mockTicketService.Setup(s => s.ObtenerTicketAsync(3, 5)).ReturnsAsync(detalle);

        // Act
        var resultado = await _controller.ObtenerTicket(3);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(detalle, okResult.Value);
    }

    // =========================================================================
    // PRUEBA 05: GET {id} - Falla si el ticket no existe (404)
    // =========================================================================
    [Fact]
    public async Task ObtenerTicket_NoExiste_DebeRetornarNotFound()
    {
        // Arrange
        SimularUsuarioAutenticado("5");
        _mockTicketService.Setup(s => s.ObtenerTicketAsync(99, 5))
            .ThrowsAsync(new KeyNotFoundException("El ticket no existe."));

        // Act
        var resultado = await _controller.ObtenerTicket(99);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    // =========================================================================
    // PRUEBA 06: GET {id} - Falla si NO es el dueño (403)
    // =========================================================================
    [Fact]
    public async Task ObtenerTicket_NoEsDueno_DebeRetornarForbidden()
    {
        // Arrange
        SimularUsuarioAutenticado("5");
        _mockTicketService.Setup(s => s.ObtenerTicketAsync(3, 5))
            .ThrowsAsync(new UnauthorizedAccessException("Acceso denegado"));

        // Act
        var resultado = await _controller.ObtenerTicket(3);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(resultado);
        Assert.Equal(StatusCodes.Status403Forbidden, statusResult.StatusCode);
    }

    // =========================================================================
    // PRUEBA 07: POST {id}/mensajes - Éxito
    // =========================================================================
    [Fact]
    public async Task Responder_MensajeValido_DebeRetornarOk()
    {
        // Arrange
        SimularUsuarioAutenticado("5");
        var dto = new TicketMensajeCreateDto { Mensaje = "Gracias por la ayuda." };

        // Act
        var resultado = await _controller.Responder(3, dto);

        // Assert
        Assert.IsType<OkObjectResult>(resultado);
        _mockTicketService.Verify(s => s.ResponderTicketAsync(3, dto, 5), Times.Once);
    }

    // =========================================================================
    // PRUEBA 08: POST {id}/mensajes - Ticket cerrado (400)
    // =========================================================================
    [Fact]
    public async Task Responder_TicketCerrado_DebeRetornarBadRequest()
    {
        // Arrange
        SimularUsuarioAutenticado("5");
        var dto = new TicketMensajeCreateDto { Mensaje = "¿Pueden reabrir?" };

        _mockTicketService.Setup(s => s.ResponderTicketAsync(3, dto, 5))
            .ThrowsAsync(new AutoMarket.Core.Exceptions.BusinessRuleException("Este ticket está cerrado."));

        // Act
        var resultado = await _controller.Responder(3, dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    // =========================================================================
    // PRUEBA 09: POST {id}/cerrar - Éxito
    // =========================================================================
    [Fact]
    public async Task Cerrar_TicketValido_DebeRetornarOk()
    {
        // Arrange
        SimularUsuarioAutenticado("5");

        // Act
        var resultado = await _controller.Cerrar(3);

        // Assert
        Assert.IsType<OkObjectResult>(resultado);
        _mockTicketService.Verify(s => s.CerrarTicketAsync(3, 5), Times.Once);
    }
}
