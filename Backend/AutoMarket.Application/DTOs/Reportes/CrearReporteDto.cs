using AutoMarket.Core.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Application.DTOs.Reportes;

public class CrearReporteDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El anuncio es inválido.")]
    public int AnuncioId { get; set; }

    [Required(ErrorMessage = "Debes seleccionar un motivo.")]
    public ReporteMotivo Motivo { get; set; }

    [StringLength(500, ErrorMessage = "El detalle no puede exceder los 500 caracteres.")]
    public string? Detalle { get; set; }
}
