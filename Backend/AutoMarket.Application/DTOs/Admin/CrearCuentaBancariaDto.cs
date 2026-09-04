using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Application.DTOs.Admin;

public class CrearCuentaBancariaDto
{
    public BancoDestino Banco { get; set; }
    public string NombreTitular { get; set; } = string.Empty;
    public string NumeroCuenta { get; set; } = string.Empty;
    public string TipoCuenta { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string ConceptoReferencia { get; set; } = string.Empty;
}
