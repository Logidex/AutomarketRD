using System.Security.Claims;
using AutoMarket.API.Controllers;
using AutoMarket.Application.DTOs.Encuestas;
using AutoMarket.Application.Features.Encuestas.Commands;
using AutoMarket.Application.Features.Encuestas.Queries;
using AutoMarket.Core.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AutoMarket.Tests.Controllers;

public class EncuestasControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly EncuestasController _controller;

    public EncuestasControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new EncuestasController(_mockMediator.Object);
    }

    private void SimularUsuarioAutenticado(string usuarioId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task ObtenerActiva_EncuestaExiste_DebeRetornarOk()
    {
        SimularUsuarioAutenticado("15");
        var encuesta = new EncuestaActivaDto { Id = 1, Titulo = "Satisfacción" };
        _mockMediator.Setup(s => s.Send(It.IsAny<ObtenerEncuestaActivaQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(encuesta);

        var resultado = await _controller.ObtenerActiva();

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(encuesta, okResult.Value);
    }

    [Fact]
    public async Task ObtenerActiva_NoExiste_DebeRetornarNotFound()
    {
        SimularUsuarioAutenticado("15");
        _mockMediator.Setup(s => s.Send(It.IsAny<ObtenerEncuestaActivaQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EncuestaActivaDto?)null);

        var resultado = await _controller.ObtenerActiva();

        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    [Fact]
    public async Task Responder_DatosValidos_DebeRetornarOk()
    {
        SimularUsuarioAutenticado("15");
        var dto = new ResponderEncuestaDto { EncuestaId = 1, Respuestas = new List<RespuestaEncuestaDto>() };
        _mockMediator.Setup(s => s.Send(It.IsAny<ResponderEncuestaCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var resultado = await _controller.Responder(dto);

        Assert.IsType<OkObjectResult>(resultado);
    }

    [Fact]
    public async Task Responder_ReglaViolada_DebeLanzarBusinessRuleException()
    {
        SimularUsuarioAutenticado("15");
        var dto = new ResponderEncuestaDto { EncuestaId = 1, Respuestas = new List<RespuestaEncuestaDto>() };
        _mockMediator.Setup(s => s.Send(It.IsAny<ResponderEncuestaCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException("Ya respondiste esta encuesta."));

        await Assert.ThrowsAsync<BusinessRuleException>(() => _controller.Responder(dto));
    }
}
