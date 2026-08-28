using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Application.DTOs.Encuestas;

public class ResponderEncuestaDto
{
    [Range(1, int.MaxValue, ErrorMessage = "La encuesta es inválida.")]
    public int EncuestaId { get; set; }

    [Required(ErrorMessage = "Faltan las respuestas de la encuesta.")]
    [MinLength(1, ErrorMessage = "Faltan las respuestas de la encuesta.")]
    public List<RespuestaEncuestaDto> Respuestas { get; set; } = new();
}

public class RespuestaEncuestaDto
{
    [Range(1, int.MaxValue, ErrorMessage = "La pregunta es inválida.")]
    public int PreguntaId { get; set; }

    /// <summary>Escala 1-5 (obligatoria para preguntas de tipo Escala).</summary>
    [Range(1, 5, ErrorMessage = "La escala debe estar entre 1 y 5.")]
    public int? ValorEscala { get; set; }

    /// <summary>Texto libre (opcional para preguntas de tipo Abierta).</summary>
    [MaxLength(1000, ErrorMessage = "El comentario no puede superar los 1000 caracteres.")]
    public string? ValorTexto { get; set; }
}
