namespace AutoMarket.Application.DTOs
{
    public class AnuncioSearchDto
    {
        public int? UsuarioId { get; set; }
        // 1. Textos y categorías opcionales
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public string? TipoVehiculo { get; set; }
        public string? ColorExterior { get; set; }
        public string? ColorInterior { get; set; }
        public string? Transmision { get; set; }
        public string? Combustible { get; set; }
        public string? Ubicacion { get; set; }

        // 2. Rangos de búsqueda numéricos
        public int? AnioDesde { get; set; }
        public int? AnioHasta { get; set; }
        
        public decimal? PrecioMinimo { get; set; }
        public decimal? PrecioMaximo { get; set; }
        
        public int? KilometrajeMaximo { get; set; }

        // 3. Paginación con valores por defecto inquebrantables
        public int PaginaActual { get; set; } = 1;
        public int CantidadAnuncios { get; set; } = 20;
    }
}