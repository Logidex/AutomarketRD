using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Application.DTOs.Encuestas;

public class EncuestaActivaDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    /// <summary>El usuario ya respondió esta encuesta (no volver a mostrarla).</summary>
    public bool YaRespondio { get; set; }

    public List<EncuestaPreguntaDto> Preguntas { get; set; } = new();
}

public class EncuestaPreguntaDto
{
    public int Id { get; set; }
    public string Texto { get; set; } = string.Empty;
    public int Orden { get; set; }
    public TipoPreguntaEncuesta Tipo { get; set; }
}
