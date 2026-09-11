using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace AutoMarket.Application.Services;

/// <summary>
/// Servicio de redimensionamiento de imágenes para AdSlots.
/// Utiliza SixLabors.ImageSharp para procesar imágenes subidas por dealers.
/// </summary>
public class AdSlotImageService
{
    private readonly IAlmacenadorArchivos _almacenadorArchivos;

    public AdSlotImageService(IAlmacenadorArchivos almacenadorArchivos)
    {
        _almacenadorArchivos = almacenadorArchivos;
    }

    /// <summary>
    /// Dimensiones máximas recomendadas por ubicación (ancho x alto en px).
    /// El servicio redimensiona manteniendo la relación de aspecto.
    /// </summary>
    private static (int Ancho, int Alto) ObtenerDimensionesObjetivo(UbicacionAdSlot ubicacion)
    {
        return ubicacion switch
        {
            UbicacionAdSlot.HomepageLateral => (600, 1200),
            UbicacionAdSlot.HomepageBuscador => (1200, 400),
            UbicacionAdSlot.HomepageFooter => (1400, 200),
            UbicacionAdSlot.VehiculosLateral => (600, 500),
            UbicacionAdSlot.VehiculosGrid => (600, 500),
            UbicacionAdSlot.VehiculosFooter => (1400, 200),
            UbicacionAdSlot.DetalleLateral => (600, 500),
            UbicacionAdSlot.DetalleFooter => (1400, 200),
            UbicacionAdSlot.AgenciasLateral => (600, 500),
            UbicacionAdSlot.AgenciasFooter => (1400, 200),
            _ => (600, 500)
        };
    }

    /// <summary>
    /// Procesa una imagen subida: redimensiona y guarda tanto la original
    /// como la versión optimizada en S3.
    /// </summary>
    /// <param name="stream">Stream de la imagen original</param>
    /// <param name="nombreArchivo">Nombre original del archivo</param>
    /// <param name="contentType">MIME type de la imagen</param>
    /// <param name="perfilDealerId">ID del dealer que sube la imagen</param>
    /// <param name="ubicacion">Ubicación del slot destino</param>
    /// <returns>(ClaveOriginal, ClaveRedimensionada)</returns>
    public async Task<(string Original, string Redimensionada)> ProcesarImagenAsync(
        Stream stream,
        string nombreArchivo,
        string contentType,
        int perfilDealerId,
        UbicacionAdSlot ubicacion)
    {
        var extension = Path.GetExtension(nombreArchivo).ToLowerInvariant();
        var guid = Guid.NewGuid().ToString("N");

        var claveOriginal = $"adslots/{perfilDealerId}-{guid}-original{extension}";
        var claveRedimensionada = $"adslots/{perfilDealerId}-{guid}-opt.webp";

        // Guardar original tal cual
        stream.Position = 0;
        await _almacenadorArchivos.GuardarArchivoAsync(stream, claveOriginal, contentType);

        // Redimensionar
        stream.Position = 0;
        var (ancho, alto) = ObtenerDimensionesObjetivo(ubicacion);

        using var image = await Image.LoadAsync(stream);

        // Calcular recorte centrado (crop to fill)
        var ratioAncho = (double)ancho / image.Width;
        var ratioAlto = (double)alto / image.Height;
        var ratio = Math.Max(ratioAncho, ratioAlto);

        var nuevoAncho = (int)(image.Width * ratio);
        var nuevoAlto = (int)(image.Height * ratio);

        image.Mutate(x => x
            .Resize(nuevoAncho, nuevoAlto)
            .Crop(new Rectangle(
                (nuevoAncho - ancho) / 2,
                (nuevoAlto - alto) / 2,
                ancho,
                alto)));

        using var outputStream = new MemoryStream();
        await image.SaveAsWebpAsync(outputStream, new SixLabors.ImageSharp.Formats.Webp.WebpEncoder
        {
            Quality = 85
        });

        outputStream.Position = 0;
        await _almacenadorArchivos.GuardarArchivoAsync(outputStream, claveRedimensionada, "image/webp");

        return (claveOriginal, claveRedimensionada);
    }
}
