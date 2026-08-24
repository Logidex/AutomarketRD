using AutoMarket.Application.DTOs.Encuestas;

namespace AutoMarket.Application.Interfaces;

public interface IEncuestaService
{
    /// <summary>Encuesta activa para mostrar al usuario (con flag de ya respondida).</summary>
    Task<EncuestaActivaDto?> ObtenerActivaAsync(int usuarioId);

    /// <summary>Registra las respuestas del usuario (una sola vez por encuesta).</summary>
    Task ResponderAsync(int usuarioId, ResponderEncuestaDto dto);

    /// <summary>Resultados agregados para el panel de administración.</summary>
    Task<EncuestaResultadosDto> ObtenerResultadosAsync(int encuestaId);
}
