namespace AutoMarket.Application.DTOs.Usuario
{
    public class PerfilDealerPublicoDto
    {
        public int Id { get; set; }
        public string NombreAgencia { get; set; } = null!;
        public string? LogoUrl { get; set; }
        public string? Horarios { get; set; }
        public string Ubicacion { get; set; } = null!;
        public string TelefonoAgencia { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public string? WhatsApp { get; set; }
    }
}