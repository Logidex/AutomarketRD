using Moq;
using Xunit;
using AutoMarket.Application.DTOs.Encuestas;
using AutoMarket.Application.Services;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Tests.Services;

public class EncuestaServiceTests
{
    private const int UsuarioId = 7;

    private readonly Mock<IEncuestaRepository> _mockEncuestaRepo;
    private readonly EncuestaService _servicio;

    public EncuestaServiceTests()
    {
        _mockEncuestaRepo = new Mock<IEncuestaRepository>();
        _servicio = new EncuestaService(_mockEncuestaRepo.Object);
    }

    /// <summary>Los Ids los asigna la BD; en memoria los fijamos por reflexión.</summary>
    private static void AsignarId(EncuestaPregunta pregunta, int id) =>
        typeof(EncuestaPregunta).GetProperty(nameof(EncuestaPregunta.Id))!.SetValue(pregunta, id);

    private static Encuesta CrearEncuestaConIds()
    {
        var encuesta = new Encuesta(
            "¿Cómo es tu experiencia en AutoMarket RD?",
            "Tu opinión nos ayuda a mejorar.");

        encuesta.AgregarPregunta("¿Qué tan satisfecho estás?", TipoPreguntaEncuesta.Escala);
        encuesta.AgregarPregunta("¿Qué tan fácil es encontrar vehículos?", TipoPreguntaEncuesta.Escala);
        encuesta.AgregarPregunta("¿Qué cambiarías? (opcional)", TipoPreguntaEncuesta.Abierta);

        for (var i = 0; i < encuesta.Preguntas.Count; i++)
        {
            AsignarId(encuesta.Preguntas[i], i + 1);
        }

        return encuesta;
    }

    private static ResponderEncuestaDto CrearEnvioCompleto(int encuestaId) =>
        new()
        {
            EncuestaId = encuestaId,
            Respuestas = new List<RespuestaEncuestaDto>
            {
                new() { PreguntaId = 1, ValorEscala = 5 },
                new() { PreguntaId = 2, ValorEscala = 3 },
                new() { PreguntaId = 3, ValorTexto = "Más filtros de búsqueda" }
            }
        };

    private void ConfigurarEncuestaActiva(Encuesta encuesta, bool yaRespondio = false)
    {
        _mockEncuestaRepo.Setup(r => r.ObtenerPorIdAsync(encuesta.Id)).ReturnsAsync(encuesta);
        _mockEncuestaRepo.Setup(r => r.YaRespondioAsync(encuesta.Id, UsuarioId)).ReturnsAsync(yaRespondio);
    }

    // =========================================================================
    // ObtenerActiva
    // =========================================================================
    [Fact]
    public async Task ObtenerActiva_ConEncuestaVigente_DebeRetornarDtoOrdenado()
    {
        // ARRANGE
        var encuesta = CrearEncuestaConIds();
        _mockEncuestaRepo.Setup(r => r.ObtenerActivaAsync()).ReturnsAsync(encuesta);
        _mockEncuestaRepo.Setup(r => r.YaRespondioAsync(encuesta.Id, UsuarioId)).ReturnsAsync(false);

        // ACT
        var dto = await _servicio.ObtenerActivaAsync(UsuarioId);

        // ASSERT
        Assert.NotNull(dto);
        Assert.False(dto!.YaRespondio);
        Assert.Equal(3, dto.Preguntas.Count);
        Assert.Equal(1, dto.Preguntas[0].Orden);
        Assert.Equal(TipoPreguntaEncuesta.Escala, dto.Preguntas[0].Tipo);
        Assert.Equal(TipoPreguntaEncuesta.Abierta, dto.Preguntas[2].Tipo);
    }

    [Fact]
    public async Task ObtenerActiva_SinEncuestas_DebeRetornarNull()
    {
        // ARRANGE
        _mockEncuestaRepo.Setup(r => r.ObtenerActivaAsync()).ReturnsAsync((Encuesta?)null);

        // ACT
        var dto = await _servicio.ObtenerActivaAsync(UsuarioId);

        // ASSERT
        Assert.Null(dto);
    }

    // =========================================================================
    // Responder
    // =========================================================================
    [Fact]
    public async Task Responder_EnvioCompleto_DebeGuardarUnaRespuestaPorPregunta()
    {
        // ARRANGE
        var encuesta = CrearEncuestaConIds();
        ConfigurarEncuestaActiva(encuesta);

        List<EncuestaRespuesta>? guardadas = null;
        _mockEncuestaRepo
            .Setup(r => r.AgregarRespuestasAsync(It.IsAny<IReadOnlyList<EncuestaRespuesta>>()))
            .Callback<IReadOnlyList<EncuestaRespuesta>>(r => guardadas = r.ToList())
            .Returns(Task.CompletedTask);

        // ACT
        await _servicio.ResponderAsync(UsuarioId, CrearEnvioCompleto(encuesta.Id));

        // ASSERT
        Assert.NotNull(guardadas);
        Assert.Equal(3, guardadas!.Count);
        Assert.All(guardadas, r => Assert.Equal(UsuarioId, r.UsuarioId));
        Assert.Contains(guardadas, r => r.PreguntaId == 1 && r.ValorEscala == 5);
        Assert.Contains(guardadas, r => r.PreguntaId == 3 && r.ValorTexto == "Más filtros de búsqueda");
    }

    [Fact]
    public async Task Responder_YaRespondio_DebeLanzarBusinessRule()
    {
        // ARRANGE
        var encuesta = CrearEncuestaConIds();
        ConfigurarEncuestaActiva(encuesta, yaRespondio: true);

        // ACT + ASSERT
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _servicio.ResponderAsync(UsuarioId, CrearEnvioCompleto(encuesta.Id)));
        Assert.Contains("Ya respondiste", ex.Message);
    }

    [Fact]
    public async Task Responder_EncuestaInexistente_DebeLanzarBusinessRule()
    {
        // ARRANGE
        _mockEncuestaRepo
            .Setup(r => r.ObtenerPorIdAsync(999))
            .ReturnsAsync((Encuesta?)null);

        // ACT + ASSERT
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _servicio.ResponderAsync(UsuarioId, CrearEnvioCompleto(999)));
        Assert.Contains("no está disponible", ex.Message);
    }

    [Fact]
    public async Task Responder_FaltaUnaPregunta_DebeLanzarBusinessRule()
    {
        // ARRANGE
        var encuesta = CrearEncuestaConIds();
        ConfigurarEncuestaActiva(encuesta);

        var incompleto = new ResponderEncuestaDto
        {
            EncuestaId = encuesta.Id,
            Respuestas = new List<RespuestaEncuestaDto>
            {
                new() { PreguntaId = 1, ValorEscala = 4 }
            }
        };

        // ACT + ASSERT
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _servicio.ResponderAsync(UsuarioId, incompleto));
        Assert.Contains("todas las preguntas", ex.Message);
    }

    [Fact]
    public async Task Responder_EscalaFueraDeRango_DebeLanzarBusinessRule()
    {
        // ARRANGE
        var encuesta = CrearEncuestaConIds();
        ConfigurarEncuestaActiva(encuesta);

        var invalido = new ResponderEncuestaDto
        {
            EncuestaId = encuesta.Id,
            Respuestas = new List<RespuestaEncuestaDto>
            {
                new() { PreguntaId = 1 }, // sin valor de escala
                new() { PreguntaId = 2, ValorEscala = 4 },
                new() { PreguntaId = 3, ValorTexto = "" }
            }
        };

        // ACT + ASSERT
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _servicio.ResponderAsync(UsuarioId, invalido));
        Assert.Contains("1 al 5", ex.Message);
    }

    [Fact]
    public async Task Responder_ConPreguntasAjenas_DebeLanzarBusinessRule()
    {
        // ARRANGE: envío con más preguntas de las que tiene la encuesta
        var encuesta = CrearEncuestaConIds();
        ConfigurarEncuestaActiva(encuesta);

        var conAjena = CrearEnvioCompleto(encuesta.Id);
        conAjena.Respuestas.Add(new RespuestaEncuestaDto { PreguntaId = 99, ValorEscala = 2 });

        // ACT + ASSERT
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _servicio.ResponderAsync(UsuarioId, conAjena));
        Assert.Contains("no pertenecen", ex.Message);
    }

    // =========================================================================
    // Resultados (admin)
    // =========================================================================
    [Fact]
    public async Task ObtenerResultados_DebeAgregarPromediosDistribucionYComentarios()
    {
        // ARRANGE
        var encuesta = CrearEncuestaConIds();
        _mockEncuestaRepo.Setup(r => r.ObtenerPorIdAsync(encuesta.Id)).ReturnsAsync(encuesta);

        var respuestas = new List<EncuestaRespuesta>
        {
            new(encuesta.Id, 1, 1, 5, null),
            new(encuesta.Id, 1, 2, 4, null),
            new(encuesta.Id, 1, 3, 5, null),
            new(encuesta.Id, 3, 1, null, "Más fotos"),
            new(encuesta.Id, 3, 2, null, "App móvil")
        };
        _mockEncuestaRepo.Setup(r => r.ObtenerRespuestasAsync(encuesta.Id)).ReturnsAsync(respuestas);

        // ACT
        var resultados = await _servicio.ObtenerResultadosAsync(encuesta.Id);

        // ASSERT
        Assert.Equal(3, resultados.TotalRespondentes);

        var escala = resultados.Preguntas.First(p => p.Tipo == TipoPreguntaEncuesta.Escala);
        Assert.Equal(4.67m, escala.PromedioEscala);
        Assert.Equal(2, escala.Distribucion[5]);
        Assert.Equal(1, escala.Distribucion[4]);

        var abierta = resultados.Preguntas.First(p => p.Tipo == TipoPreguntaEncuesta.Abierta);
        Assert.Equal(2, abierta.Comentarios.Count);
    }
}
