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

        // true si es una cuenta Vendedor (particular, un solo anuncio),
        // false si es un Dealer con perfil de agencia.
        public bool EsVendedorParticular { get; set; }

        // true si tiene correo confirmado y suscripción pagada (no Gratis).
        public bool EsDealerVerificado { get; set; }
    }
}