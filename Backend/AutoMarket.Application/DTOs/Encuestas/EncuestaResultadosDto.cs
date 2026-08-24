using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Application.DTOs.Encuestas;

/// <summary>Resultados agregados de una encuesta (vista admin).</summary>
public class EncuestaResultadosDto
{
    public int EncuestaId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public bool Activa { get; set; }

    /// <summary>Usuarios distintos que respondieron.</summary>
    public int TotalRespondentes { get; set; }

    public List<ResultadoPreguntaDto> Preguntas { get; set; } = new();
}

public class ResultadoPreguntaDto
{
    public int PreguntaId { get; set; }
    public string Texto { get; set; } = string.Empty;
    public TipoPreguntaEncuesta Tipo { get; set; }

    public int TotalRespuestas { get; set; }

    /// <summary>Promedio de escala (solo tipo Escala), redondeado a 2 decimales.</summary>
    public decimal? PromedioEscala { get; set; }

    /// <summary>Distribución de escala: clave = valor (1-5), valor = cantidad.</summary>
    public Dictionary<int, int> Distribucion { get; set; } = new();

    /// <summary>Comentarios escritos (solo tipo Abierta), del más reciente al más antiguo.</summary>
    public List<ComentarioEncuestaDto> Comentarios { get; set; } = new();
}

public class ComentarioEncuestaDto
{
    public DateTime FechaUtc { get; set; }
    public string Texto { get; set; } = string.Empty;
}
