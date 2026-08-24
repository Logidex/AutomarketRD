using AutoMarket.Core.Entities;

namespace AutoMarket.Core.Interfaces;

public interface IEncuestaRepository
{
    /// <summary>Obtiene la encuesta activa con sus preguntas ordenadas.</summary>
    Task<Encuesta?> ObtenerActivaAsync();

    /// <summary>Obtiene una encuesta por Id (con preguntas).</summary>
    Task<Encuesta?> ObtenerPorIdAsync(int encuestaId);

    /// <summary>Indica si el usuario ya respondió alguna pregunta de la encuesta.</summary>
    Task<bool> YaRespondioAsync(int encuestaId, int usuarioId);

    /// <summary>Todas las respuestas de una encuesta (para resultados de admin).</summary>
    Task<List<EncuestaRespuesta>> ObtenerRespuestasAsync(int encuestaId);

    /// <summary>
    /// Inserta las respuestas en una sola transacción. El índice único de BD
    /// rechaza duplicados si dos envíos compiten.
    /// </summary>
    Task AgregarRespuestasAsync(IReadOnlyList<EncuestaRespuesta> respuestas);

    /// <summary>Inserta una encuesta nueva (usado por el seeder).</summary>
    Task<Encuesta> AgregarAsync(Encuesta encuesta);
}
