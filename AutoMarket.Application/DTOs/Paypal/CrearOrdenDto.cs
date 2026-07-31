namespace AutoMarket.Application.DTOs.Paypal;

public class CrearOrdenDto
{
    public decimal Monto { get; set; }
    public string NombrePlan { get; set; } = string.Empty;
    public string Ciclo { get; set; } = string.Empty;
}