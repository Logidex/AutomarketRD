using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Lead;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.API.Controllers;
using AutoMarket.Application.Features.Leads.Commands;
using AutoMarket.Application.Features.Leads.Queries;
using MediatR;

namespace AutoMarket.Tests.Controllers;

public class LeadsControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly LeadsController _controller;

    public LeadsControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new LeadsController(_mockMediator.Object);
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
    public async Task CrearLead_DatosValidos_DebeRetornarOkConMensaje()
    {
        var dto = new LeadCreateDto
        {
            AnuncioId = 1,
            NombreContacto = "Carlos",
            Mensaje = "Me interesa",
            Canal = CanalContacto.Formulario
        };

        var resultado = await _controller.CrearLead(dto);

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var value = okResult.Value;
        var propMensaje = value?.GetType().GetProperty("mensaje")?.GetValue(value, null);
        Assert.Equal("Tu mensaje ha sido enviado exitosamente al vendedor.", propMensaje);
    }

    [Fact]
    public async Task CrearLead_DatosInvalidos_DebeRetornarBadRequest()
    {
        var dto = new LeadCreateDto { AnuncioId = 1 };
        _controller.ModelState.AddModelError("NombreContacto", "El nombre es obligatorio.");

        var resultado = await _controller.CrearLead(dto);

        Assert.IsType<BadRequestObjectResult>(resultado);
        _mockMediator.Verify(s => s.Send(It.IsAny<CrearLeadCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ObtenerPorAnuncio_DueñoDelAnuncio_DebeRetornarOkConLista()
    {
        int anuncioId = 10;
        int usuarioLogueado = 5;
        SimularUsuarioAutenticado(usuarioLogueado.ToString());

        var listaSimulada = new List<Lead>();
        _mockMediator
            .Setup(s => s.Send(It.Is<ObtenerLeadsPorAnuncioQuery>(q => q.AnuncioId == anuncioId && q.UsuarioId == usuarioLogueado), It.IsAny<CancellationToken>()))
            .ReturnsAsync(listaSimulada);

        var resultado = await _controller.ObtenerPorAnuncio(anuncioId);

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(listaSimulada, okResult.Value);
    }

    [Fact]
    public async Task ObtenerPorAnuncio_NoEsDueño_DebeRetornarForbidden()
    {
        int anuncioId = 10;
        int usuarioLogueado = 5;
        SimularUsuarioAutenticado(usuarioLogueado.ToString());

        _mockMediator
            .Setup(s => s.Send(It.IsAny<ObtenerLeadsPorAnuncioQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Acceso denegado"));

        var resultado = await _controller.ObtenerPorAnuncio(anuncioId);

        var statusResult = Assert.IsType<ObjectResult>(resultado);
        Assert.Equal(StatusCodes.Status403Forbidden, statusResult.StatusCode);
    }

    [Fact]
    public async Task ObtenerPorAnuncio_SinIdentidadEnToken_DebeRetornarForbidden()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var resultado = await _controller.ObtenerPorAnuncio(10);

        var statusResult = Assert.IsType<ObjectResult>(resultado);
        Assert.Equal(StatusCodes.Status403Forbidden, statusResult.StatusCode);
    }

    [Fact]
    public async Task ObtenerMisLeads_Sin_IdentidadEnToken_DebeLanzarExcepcionNoAutorizado()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _controller.ObtenerMisLeads());
    }

    [Fact]
    public async Task ObtenerMisLeads_ConTokenValido_DebeExtraerIdYRetornarOk()
    {
        int dealerIdLogueado = 5;
        SimularUsuarioAutenticado(dealerIdLogueado.ToString());

        var listaSimulada = new List<LeadDealerDto>();
        _mockMediator.Setup(s => s.Send(
            It.Is<ObtenerLeadsPorDealerQuery>(q => q.DealerId == dealerIdLogueado),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(listaSimulada);

        var resultado = await _controller.ObtenerMisLeads();

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(listaSimulada, okResult.Value);
    }
}
