using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Application.Services;

/// <summary>
/// Servicio de imágenes para AdSlots.
/// Guarda la imagen subida tal cual (sin resize server-side).
/// El frontend maneja el display con object-fit.
/// </summary>
public class AdSlotImageService
{
    private readonly IAlmacenadorArchivos _almacenadorArchivos;

    public AdSlotImageService(IAlmacenadorArchivos almacenadorArchivos)
    {
        _almacenadorArchivos = almacenadorArchivos;
    }

    /// <summary>
    /// Guarda la imagen subida en S3.
    /// </summary>
    /// <param name="stream">Stream de la imagen original</param>
    /// <param name="nombreArchivo">Nombre original del archivo</param>
    /// <param name="contentType">MIME type de la imagen</param>
    /// <param name="perfilDealerId">ID del dealer que sube la imagen</param>
    /// <param name="ubicacion">Ubicación del slot destino</param>
    /// <returns>(ClaveOriginal, ClaveRedimensionada) — ambas apuntan a la misma imagen</returns>
    public async Task<(string Original, string Redimensionada)> ProcesarImagenAsync(
        Stream stream,
        string nombreArchivo,
        string contentType,
        int perfilDealerId,
        UbicacionAdSlot ubicacion)
    {
        var extension = Path.GetExtension(nombreArchivo).ToLowerInvariant();
        var guid = Guid.NewGuid().ToString("N");

        var clave = $"adslots/{perfilDealerId}-{guid}{extension}";

        stream.Position = 0;
        var claveAlmacenada = await _almacenadorArchivos.GuardarArchivoAsync(stream, clave, contentType);

        return (claveAlmacenada, claveAlmacenada);
    }
}
