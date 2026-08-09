using Microsoft.AspNetCore.Http;

namespace AutoMarket.Application.DTOs.Usuario
{
    public class PerfilDealerUpdateDto
    {
        // Datos editables
        public string? NombreAgencia { get; set; }
        public string? Ubicacion { get; set; }
        public string? TelefonoAgencia { get; set; }
        public string? Horarios { get; set; }
        public string? Descripcion { get; set; }
        public string? WhatsApp { get; set; }

        public IFormFile? Logo { get; set; } 
    }
}