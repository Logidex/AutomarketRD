using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Core.Entities;

/// <summary>
/// Encuesta para usuarios de la plataforma. V1: una sola encuesta activa
/// con preguntas fijas (sembradas); cada usuario responde una sola vez.
/// </summary>
public class Encuesta
{
    public int Id { get; private set; }
    public string Titulo { get; private set; } = null!;
    public string? Descripcion { get; private set; }
    public bool Activa { get; private set; }
    public DateTime FechaCreacionUtc { get; private set; }

    public virtual List<EncuestaPregunta> Preguntas { get; private set; } = new();

    private Encuesta() { }

    public Encuesta(string titulo, string? descripcion = null)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new ArgumentException("El título de la encuesta es obligatorio.", nameof(titulo));

        Titulo = titulo.Trim();
        Descripcion = descripcion?.Trim();
        Activa = true;
        FechaCreacionUtc = DateTime.UtcNow;
    }

    /// <summary>Agrega una pregunta manteniendo el orden secuencial.</summary>
    public void AgregarPregunta(string texto, TipoPreguntaEncuesta tipo)
    {
        if (string.IsNullOrWhiteSpace(texto))
            throw new ArgumentException("El texto de la pregunta es obligatorio.", nameof(texto));

        Preguntas.Add(new EncuestaPregunta(this, texto, tipo, Preguntas.Count + 1));
    }

    /// <summary>Desactiva la encuesta: deja de mostrarse y no acepta respuestas.</summary>
    public void Desactivar()
    {
        Activa = false;
    }

    /// <summary>Indica si la pregunta pertenece a esta encuesta.</summary>
    public bool ContienePregunta(int preguntaId)
    {
        return Preguntas.Any(p => p.Id == preguntaId);
    }
}
