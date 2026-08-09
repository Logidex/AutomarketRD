using System.ComponentModel.DataAnnotations;
using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Application.DTOs;

public class LeadCreateDto
{
    [Required(ErrorMessage = "El identificador del vehículo es obligatorio.")]
    public int AnuncioId { get; set; }

    [Required(ErrorMessage = "Por favor, ingresa tu nombre para que el vendedor pueda contactarte.")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
    public string NombreContacto { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede exceder los 150 caracteres.")]
    public string? EmailContacto { get; set; }

    [StringLength(20, ErrorMessage = "El número de teléfono no puede exceder los 20 caracteres.")]
    public string? TelefonoContacto { get; set; }

    [Required(ErrorMessage = "El mensaje para el vendedor es obligatorio.")]
    [StringLength(1000, ErrorMessage = "El mensaje es demasiado largo. El máximo permitido es 1000 caracteres.")]
    public string Mensaje { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes especificar el canal de contacto.")]
    public CanalContacto Canal { get; set; }
}