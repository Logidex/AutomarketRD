using Xunit;
using Moq;
using AutoMarket.Application.Features.Encuestas.Handlers;
using AutoMarket.Application.Features.Encuestas.Commands;
using AutoMarket.Application.Features.Encuestas.Queries;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.DTOs.Encuestas;

namespace AutoMarket.Tests.Features.Encuestas;

public class EncuestaHandlerTests
{
    private readonly Mock<IEncuestaService> _mockService;
    private readonly EncuestaCommandHandler _commandHandler;
    private readonly EncuestaQueryHandler _queryHandler;

    public EncuestaHandlerTests()
    {
        _mockService = new Mock<IEncuestaService>();
        _commandHandler = new EncuestaCommandHandler(_mockService.Object);
        _queryHandler = new EncuestaQueryHandler(_mockService.Object);
    }

    [Fact]
    public async Task Handle_ResponderEncuesta_DebeLlamarServicio()
    {
        var dto = new ResponderEncuestaDto
        {
            EncuestaId = 1,
            Respuestas = new List<RespuestaEncuestaDto>
            {
                new() { PreguntaId = 1, ValorEscala = 5 },
                new() { PreguntaId = 2, ValorTexto = "Excelente servicio" }
            }
        };
        var command = new ResponderEncuestaCommand(UsuarioId: 10, Dto: dto);
        _mockService
            .Setup(s => s.ResponderAsync(10, dto))
            .Returns(Task.CompletedTask);

        await _commandHandler.Handle(command, CancellationToken.None);

        _mockService.Verify(s => s.ResponderAsync(10, dto), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerEncuestaActiva_DebeRetornarDto()
    {
        var query = new ObtenerEncuestaActivaQuery(UsuarioId: 10);
        var expected = new EncuestaActivaDto
        {
            Id = 1,
            Titulo = "Satisfacción del servicio",
            YaRespondio = false,
            Preguntas = new List<EncuestaPreguntaDto>
            {
                new() { Id = 1, Texto = "¿Cómo calificarías?", Orden = 1 }
            }
        };
        _mockService
            .Setup(s => s.ObtenerActivaAsync(10))
            .ReturnsAsync(expected);

        var resultado = await _queryHandler.Handle(query, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal("Satisfacción del servicio", resultado!.Titulo);
        Assert.False(resultado.YaRespondio);
        Assert.Single(resultado.Preguntas);
        _mockService.Verify(s => s.ObtenerActivaAsync(10), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerEncuestaActiva_Nula_DebeRetornarNull()
    {
        var query = new ObtenerEncuestaActivaQuery(UsuarioId: 99);
        _mockService
            .Setup(s => s.ObtenerActivaAsync(99))
            .ReturnsAsync((EncuestaActivaDto?)null);

        var resultado = await _queryHandler.Handle(query, CancellationToken.None);

        Assert.Null(resultado);
        _mockService.Verify(s => s.ObtenerActivaAsync(99), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerResultadosEncuesta_DebeRetornarDto()
    {
        var query = new ObtenerResultadosEncuestaQuery(EncuestaId: 1);
        var expected = new EncuestaResultadosDto
        {
            EncuestaId = 1,
            Titulo = "Satisfacción del servicio",
            Activa = true,
            TotalRespondentes = 50,
            Preguntas = new List<ResultadoPreguntaDto>
            {
                new() { PreguntaId = 1, Texto = "¿Cómo calificarías?", TotalRespuestas = 50, PromedioEscala = 4.2m }
            }
        };
        _mockService
            .Setup(s => s.ObtenerResultadosAsync(1))
            .ReturnsAsync(expected);

        var resultado = await _queryHandler.Handle(query, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal(50, resultado!.TotalRespondentes);
        Assert.Equal(4.2m, resultado.Preguntas.First().PromedioEscala);
        _mockService.Verify(s => s.ObtenerResultadosAsync(1), Times.Once);
    }
}
