using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Application.DTOs;

public class ContactoCreateDto
{
    [Required(ErrorMessage = "Por favor, ingresa tu nombre.")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Por favor, ingresa tu correo electrónico.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede exceder los 150 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Por favor, indica el asunto de tu mensaje.")]
    [StringLength(150, ErrorMessage = "El asunto no puede exceder los 150 caracteres.")]
    public string Asunto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Por favor, escribe tu mensaje.")]
    [StringLength(2000, ErrorMessage = "El mensaje no puede exceder los 2000 caracteres.")]
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>
    /// Campo trampa anti-spam (honeypot). Debe quedar vacío: los bots lo llenan,
    /// los usuarios no lo ven. Si llega con contenido, se rechaza la solicitud.
    /// </summary>
    public string? Website { get; set; }
}
