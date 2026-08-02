using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace AutoMarket.Application.DTOs;

public class AnuncioImagenUploadDto
{   
    [Required(ErrorMessage = "El identificador del anuncio es obligatorio.")]
    public int AnuncioId { get; set; }
    public int UsuarioId { get; set; }

    [Required(ErrorMessage = "Debes seleccionar al menos una imagen.")]
    [MinLength(1, ErrorMessage = "Debes subir al menos 1 imagenes.")]
    [MaxLength(10, ErrorMessage = "No puedes subir más de 10 imágenes.")]
    public List<IFormFile> Imagenes { get; set; } = new();
}