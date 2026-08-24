namespace AutoMarket.Core.Entities;

/// <summary>
/// Respuesta de un usuario a una pregunta de la encuesta. El índice único
/// (EncuestaId, UsuarioId, PreguntaId) impide respuestas duplicadas; el
/// servicio además rechaza si el usuario ya respondió CUALQUIER pregunta de
/// la encuesta (una sola vez por usuario).
/// </summary>
public class EncuestaRespuesta
{
    public int Id { get; private set; }

    public int EncuestaId { get; private set; }
    public virtual Encuesta Encuesta { get; private set; } = null!;

    public int PreguntaId { get; private set; }
    public virtual EncuestaPregunta Pregunta { get; private set; } = null!;

    public int UsuarioId { get; private set; }
    public virtual Usuario Usuario { get; private set; } = null!;

    /// <summary>Valor de escala 1-5 (solo preguntas de tipo Escala).</summary>
    public int? ValorEscala { get; private set; }

    /// <summary>Texto libre (solo preguntas de tipo Abierta).</summary>
    public string? ValorTexto { get; private set; }

    public DateTime FechaUtc { get; private set; }

    private EncuestaRespuesta() { }

    public EncuestaRespuesta(
        int encuestaId,
        int preguntaId,
        int usuarioId,
        int? valorEscala,
        string? valorTexto)
    {
        if (usuarioId <= 0)
            throw new ArgumentException("El usuario es inválido.", nameof(usuarioId));

        EncuestaId = encuestaId;
        PreguntaId = preguntaId;
        UsuarioId = usuarioId;
        ValorEscala = valorEscala;
        ValorTexto = valorTexto?.Trim();
        FechaUtc = DateTime.UtcNow;
    }
}
