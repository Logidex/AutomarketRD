using AutoMarket.Application.DTOs.Encuestas;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Application.Services;

public class EncuestaService : IEncuestaService
{
    private readonly IEncuestaRepository _encuestaRepository;

    public EncuestaService(IEncuestaRepository encuestaRepository)
    {
        _encuestaRepository = encuestaRepository;
    }

    public async Task<EncuestaActivaDto?> ObtenerActivaAsync(int usuarioId)
    {
        var encuesta = await _encuestaRepository.ObtenerActivaAsync();

        if (encuesta is null) return null;

        return new EncuestaActivaDto
        {
            Id = encuesta.Id,
            Titulo = encuesta.Titulo,
            Descripcion = encuesta.Descripcion,
            YaRespondio = await _encuestaRepository.YaRespondioAsync(encuesta.Id, usuarioId),
            Preguntas = encuesta.Preguntas
                .OrderBy(p => p.Orden)
                .Select(p => new EncuestaPreguntaDto
                {
                    Id = p.Id,
                    Texto = p.Texto,
                    Orden = p.Orden,
                    Tipo = p.Tipo
                })
                .ToList()
        };
    }

    public async Task ResponderAsync(int usuarioId, ResponderEncuestaDto dto)
    {
        var encuesta = await _encuestaRepository.ObtenerPorIdAsync(dto.EncuestaId);

        if (encuesta is null || !encuesta.Activa)
            throw new BusinessRuleException("Esta encuesta ya no está disponible.");

        if (await _encuestaRepository.YaRespondioAsync(encuesta.Id, usuarioId))
            throw new BusinessRuleException("Ya respondiste esta encuesta. ¡Gracias!");

        // Todas las preguntas de la encuesta deben estar respondidas, sin
        // duplicados y perteneciendo efectivamente a la encuesta.
        var respuestasPorPregunta = dto.Respuestas
            .GroupBy(r => r.PreguntaId)
            .ToDictionary(g => g.Key, g => g.First());

        var respuestas = new List<EncuestaRespuesta>();

        foreach (var pregunta in encuesta.Preguntas)
        {
            if (!respuestasPorPregunta.TryGetValue(pregunta.Id, out var entrada))
            {
                throw new BusinessRuleException("Responde todas las preguntas antes de enviar.");
            }

            if (pregunta.Tipo == TipoPreguntaEncuesta.Escala)
            {
                if (entrada.ValorEscala is null or < 1 or > 5)
                {
                    throw new BusinessRuleException(
                        "Las preguntas de escala requieren una calificación del 1 al 5.");
                }

                respuestas.Add(new EncuestaRespuesta(
                    encuesta.Id, pregunta.Id, usuarioId, entrada.ValorEscala, null));
            }
            else
            {
                respuestas.Add(new EncuestaRespuesta(
                    encuesta.Id, pregunta.Id, usuarioId, null, entrada.ValorTexto));
            }
        }

        if (respuestasPorPregunta.Count != encuesta.Preguntas.Count)
        {
            throw new BusinessRuleException("Hay preguntas que no pertenecen a esta encuesta.");
        }

        await _encuestaRepository.AgregarRespuestasAsync(respuestas);
    }

    public async Task<EncuestaResultadosDto> ObtenerResultadosAsync(int encuestaId)
    {
        var encuesta = await _encuestaRepository.ObtenerPorIdAsync(encuestaId);

        if (encuesta is null)
            throw new KeyNotFoundException("No se encontró la encuesta.");

        var respuestas = await _encuestaRepository.ObtenerRespuestasAsync(encuestaId);

        var resultados = new EncuestaResultadosDto
        {
            EncuestaId = encuesta.Id,
            Titulo = encuesta.Titulo,
            Activa = encuesta.Activa,
            TotalRespondentes = respuestas
                .Select(r => r.UsuarioId)
                .Distinct()
                .Count()
        };

        foreach (var pregunta in encuesta.Preguntas.OrderBy(p => p.Orden))
        {
            var deLaPregunta = respuestas.Where(r => r.PreguntaId == pregunta.Id).ToList();

            var resultado = new ResultadoPreguntaDto
            {
                PreguntaId = pregunta.Id,
                Texto = pregunta.Texto,
                Tipo = pregunta.Tipo,
                TotalRespuestas = deLaPregunta.Count
            };

            if (pregunta.Tipo == TipoPreguntaEncuesta.Escala)
            {
                resultado.Distribucion = Enumerable.Range(1, 5)
                    .ToDictionary(
                        valor => valor,
                        valor => deLaPregunta.Count(r => r.ValorEscala == valor));

                if (deLaPregunta.Count > 0)
                {
                    resultado.PromedioEscala = Math.Round(
                        (decimal)deLaPregunta.Average(r => r.ValorEscala ?? 0), 2);
                }
            }
            else
            {
                resultado.Comentarios = deLaPregunta
                    .Where(r => !string.IsNullOrWhiteSpace(r.ValorTexto))
                    .OrderByDescending(r => r.FechaUtc)
                    .Select(r => new ComentarioEncuestaDto
                    {
                        FechaUtc = r.FechaUtc,
                        Texto = r.ValorTexto!
                    })
                    .ToList();
            }

            resultados.Preguntas.Add(resultado);
        }

        return resultados;
    }
}
