using AutoMarket.Application.DTOs;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Application.Services;

/// <summary>
/// Mapeo de entidades Anuncio a DTOs.
/// Centraliza la lógica de transformación para evitar duplicación.
/// </summary>
public static class AnuncioMapper
{
    /// <summary>
    /// Mapea una entidad Anuncio a AnuncioDto (detalle completo).
    /// </summary>
    public static AnuncioDto ToDto(
        Anuncio anuncio,
        string? nombreVendedor,
        string? whatsAppContacto,
        bool esDealerVerificado,
        Usuario? vendedor = null)
    {
        return new AnuncioDto
        {
            Id = anuncio.Id,
            UsuarioId = anuncio.UsuarioId,
            NombreAnuncio = anuncio.NombreAnuncio,

            Marca = anuncio.Marca,
            Modelo = anuncio.Modelo,
            Version = anuncio.Version,

            TipoVehiculo = anuncio.TipoVehiculo,
            Motor = anuncio.Motor,
            Traccion = anuncio.Traccion,

            ColorExterior = anuncio.ColorExterior,
            ColorInterior = anuncio.ColorInterior,

            Anio = anuncio.Anio,
            Precio = anuncio.Precio,
            Moneda = anuncio.Moneda,
            PrecioAnterior = anuncio.PrecioAnterior,
            Kilometraje = anuncio.Kilometraje,

            Condicion = anuncio.Condicion,
            EnOferta = anuncio.EnOferta,

            Transmision = anuncio.Transmision,
            Combustible = anuncio.Combustible,

            Accesorios = anuncio.Accesorios.ToList(),
            Ubicacion = anuncio.Ubicacion,
            Descripcion = anuncio.Descripcion,

            Estado = anuncio.Estado,
            Fotos = anuncio.Fotos.ToList(),

            EsDestacado = anuncio.EstaDestacadoVigente,
            FechaDestacadoHasta = anuncio.FechaDestacadoHasta,

            FechaVencimiento = anuncio.FechaVencimientoUtc,

            EsDealerVerificado = esDealerVerificado,

            NombreVendedor = nombreVendedor,
            WhatsAppContacto = whatsAppContacto,

            EsVendedorParticular =
                vendedor != null &&
                string.Equals(
                    vendedor.Rol,
                    "Vendedor",
                    StringComparison.OrdinalIgnoreCase)
        };
    }

    /// <summary>
    /// Mapea una entidad Anuncio a AnuncioListadoDto (listado).
    /// </summary>
    public static AnuncioListadoDto ToListadoDto(Anuncio anuncio, bool soloPrimeraFoto)
    {
        var fotos = soloPrimeraFoto
            ? anuncio.Fotos.Take(1).ToList()
            : anuncio.Fotos != null && anuncio.Fotos.Any()
                ? anuncio.Fotos.ToList()
                : new List<string> { "url_imagen_por_defecto.jpg" };

        return new AnuncioListadoDto
        {
            Id = anuncio.Id,
            UsuarioId = anuncio.UsuarioId,
            NombreAnuncio = anuncio.NombreAnuncio,
            Marca = anuncio.Marca,
            Modelo = anuncio.Modelo,
            Version = anuncio.Version,
            TipoVehiculo = anuncio.TipoVehiculo,
            Motor = anuncio.Motor,
            Traccion = anuncio.Traccion,
            ColorExterior = anuncio.ColorExterior,
            ColorInterior = anuncio.ColorInterior,
            Anio = anuncio.Anio,
            Precio = anuncio.Precio,
            Moneda = anuncio.Moneda,
            PrecioAnterior = anuncio.PrecioAnterior,
            Kilometraje = anuncio.Kilometraje,

            Condicion = anuncio.Condicion,
            EnOferta = anuncio.EnOferta,

            Transmision = anuncio.Transmision,
            Combustible = anuncio.Combustible,
            Ubicacion = anuncio.Ubicacion,
            Estado = anuncio.Estado,
            Vistas = anuncio.Vistas,
            Fotos = fotos,
            BadgeSuscripcion = anuncio.Usuario?.PerfilDealer?.Suscripcion?.Nivel.ToString() ?? "Gratis",
            EsDestacado = anuncio.EstaDestacadoVigente,
            FechaDestacadoHasta = anuncio.FechaDestacadoHasta,
            FechaVencimiento = anuncio.FechaVencimientoUtc,
            EsDealerVerificado =
                anuncio.Usuario != null && AnuncioPlanValidator.EsDealerVerificado(anuncio.Usuario),
            CreatedAt = anuncio.CreatedAt
        };
    }

    /// <summary>
    /// Crea una entidad Anuncio desde un AnuncioCreateDto.
    /// </summary>
    public static Anuncio ToEntity(AnuncioCreateDto dto)
    {
        return new Anuncio(
            usuarioId: dto.UsuarioId,
            marca: NormalizarBusqueda(dto.Marca),
            modelo: NormalizarBusqueda(dto.Modelo),
            version: NormalizarBusqueda(dto.Version),
            tipoVehiculo: NormalizarBusqueda(dto.TipoVehiculo),
            motor: NormalizarBusqueda(dto.Motor),
            traccion: NormalizarBusqueda(dto.Traccion),
            colorExterior: NormalizarBusqueda(dto.ColorExterior),
            colorInterior: NormalizarBusqueda(dto.ColorInterior),
            anio: dto.Anio,
            precio: dto.Precio,
            moneda: dto.Moneda,
            kilometraje: dto.Kilometraje,
            transmision: NormalizarBusqueda(dto.Transmision),
            combustible: NormalizarBusqueda(dto.Combustible),
            accesorios: dto.Accesorios,
            ubicacion: NormalizarBusqueda(dto.Ubicacion),
            descripcion: dto.Descripcion
        );
    }

    /// <summary>
    /// Normaliza un campo de búsqueda (quita acentos, minúsculas).
    /// </summary>
    private static string NormalizarBusqueda(string? valor) =>
        Helpers.NormalizadorTexto.Normalizar(valor);
}
