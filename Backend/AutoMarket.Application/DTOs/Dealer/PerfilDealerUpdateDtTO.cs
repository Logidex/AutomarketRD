using Microsoft.AspNetCore.Http;

namespace AutoMarket.Application.DTOs.Usuario
{
    public class PerfilDealerUpdateDto
    {
        // Datos editables
        public string NombreAgencia { get; set; } = null!;
        public string Ubicacion { get; set; } = null!;
        public string TelefonoAgencia { get; set; } = null!;
        public string? Horarios { get; set; }
        public string Descripcion { get; set; } = null!;
        public string? WhatsApp { get; set; }

        public IFormFile? Logo { get; set; } 
    }
}