namespace AutoMarket.Application.DTOs
{
    public class AnuncioCreateDto
    {
        public int UsuarioId { get; set; }
        public string Marca { get; set; } = null!;
        public string Modelo { get; set; } = null!;
        public string TipoVehiculo { get; set; } = null!;
        public string ColorExterior { get; set; } = null!;
        public string ColorInterior { get; set; } = null!;
        public int Anio { get; set; }
        public decimal Precio { get; set; }
        public int Kilometraje { get; set; }
        public string Transmision { get; set; } = null!;
        public string Combustible { get; set; } = null!;

        public List<string> Accesorios { get; set; } = new();

        public string Ubicacion { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
    }
}