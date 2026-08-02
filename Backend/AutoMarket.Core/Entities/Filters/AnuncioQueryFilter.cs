namespace AutoMarket.Core.Entities
{
    public class AnuncioQueryFilter
    {
        public int? UsuarioId { get; set; }
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public string? TipoVehiculo { get; set; }
        public string? ColorExterior { get; set; }
        public string? ColorInterior { get; set; }
        public string? Transmision { get; set; }
        public string? Combustible { get; set; }
        public string? Ubicacion { get; set; }
        public int? AnioDesde { get; set; }
        public int? AnioHasta { get; set; }
        public decimal? PrecioMinimo { get; set; }
        public decimal? PrecioMaximo { get; set; }
        public int? KilometrajeMaximo { get; set; }
        public int PaginaActual { get; set; } = 1;
        public int CantidadPorPagina { get; set; } = 20;
    }
}