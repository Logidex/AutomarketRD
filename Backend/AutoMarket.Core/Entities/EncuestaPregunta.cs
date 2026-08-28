using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Core.Entities;

/// <summary>Pregunta de una encuesta, con orden fijo de presentación.</summary>
public class EncuestaPregunta
{
    public int Id { get; private set; }
    public int EncuestaId { get; private set; }
    public virtual Encuesta Encuesta { get; private set; } = null!;

    public string Texto { get; private set; } = null!;
    public int Orden { get; private set; }
    public TipoPreguntaEncuesta Tipo { get; private set; }

    public virtual List<EncuestaRespuesta> Respuestas { get; private set; } = new();

    private EncuestaPregunta() { }

    public EncuestaPregunta(Encuesta encuesta, string texto, TipoPreguntaEncuesta tipo, int orden)
    {
        Encuesta = encuesta ?? throw new ArgumentNullException(nameof(encuesta));

        if (string.IsNullOrWhiteSpace(texto))
            throw new ArgumentException("El texto de la pregunta es obligatorio.", nameof(texto));

        Texto = texto.Trim();
        Tipo = tipo;
        Orden = orden;
    }
}
