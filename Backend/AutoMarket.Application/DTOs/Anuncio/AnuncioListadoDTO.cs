namespace AutoMarket.Application.DTOs
{
    public class AnuncioListadoDto
    {
        public int Id { get; set; }
        public string NombreAnuncio { get; set; } = null!;
        public decimal Precio { get; set; }
        public string Ubicacion { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public List<string> Fotos { get; set; } = new();
        public string BadgeSuscripcion { get; set; } = "Gratis";
    }

}