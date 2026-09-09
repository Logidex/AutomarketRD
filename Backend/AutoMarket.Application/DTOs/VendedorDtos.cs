using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Application.DTOs;

/// <summary>
/// DTO para el perfil público de un vendedor/dealer.
/// </summary>
public class VendedorPerfilPublicoDto
{
    public int UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? LogoUrl { get; set; }
    public string? NombreAgencia { get; set; }
    public string? Direccion { get; set; }
    public bool EsVendedorParticular { get; set; }
    public bool EsDealerVerificado { get; set; }
    public int TotalAnuncios { get; set; }
}

/// <summary>
/// DTO para la información de suscripción del vendedor.
/// </summary>
public class VendedorSuscripcionDto
{
    public string Nivel { get; set; } = "Gratis";
    public string Ciclo { get; set; } = "Mensual";
    public string Estado { get; set; } = "Activa";
    public int LimiteAnuncios { get; set; }
    public int CuotaDestacados { get; set; }
    public int MaxFotos { get; set; }
    public DateTime FechaInicioUtc { get; set; }
    public DateTime FechaVencimientoUtc { get; set; }
    public int DiasRestantes { get; set; }
    public bool Activa { get; set; }
}
