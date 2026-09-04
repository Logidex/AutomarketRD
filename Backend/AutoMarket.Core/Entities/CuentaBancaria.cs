using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Core.Entities;

public class CuentaBancaria
{
    public int Id { get; set; }
    public BancoDestino Banco { get; set; }
    public string NombreTitular { get; set; } = string.Empty;
    public string NumeroCuenta { get; set; } = string.Empty;
    public string TipoCuenta { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string ConceptoReferencia { get; set; } = string.Empty;
    public bool Activa { get; set; } = true;
    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;

    public CuentaBancaria() { }

    public CuentaBancaria(
        BancoDestino banco,
        string nombreTitular,
        string numeroCuenta,
        string tipoCuenta,
        string documento,
        string conceptoReferencia)
    {
        Banco = banco;
        NombreTitular = nombreTitular;
        NumeroCuenta = numeroCuenta;
        TipoCuenta = tipoCuenta;
        Documento = documento;
        ConceptoReferencia = conceptoReferencia;
        Activa = true;
        FechaCreacionUtc = DateTime.UtcNow;
    }

    public void Actualizar(
        string nombreTitular,
        string numeroCuenta,
        string tipoCuenta,
        string documento,
        string conceptoReferencia)
    {
        NombreTitular = nombreTitular;
        NumeroCuenta = numeroCuenta;
        TipoCuenta = tipoCuenta;
        Documento = documento;
        ConceptoReferencia = conceptoReferencia;
    }

    public void ToggleActiva() => Activa = !Activa;
}
