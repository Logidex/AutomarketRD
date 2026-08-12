namespace AutoMarket.Application.Services;

public interface IAlmacenadorArchivos
{
    /// <summary>
    /// Guarda el archivo y devuelve la clave del objeto (ej. "uploads/abc.jpg"),
    /// nunca una URL pública. Para exponer el archivo al navegador usar
    /// <see cref="GenerarUrlFirmadaAsync"/>.
    /// </summary>
    Task<string> GuardarArchivoAsync(Stream stream, string nombreArchivo, string contentType);

    /// <summary>Genera una URL firmada de solo lectura con expiración corta.</summary>
    Task<string> GenerarUrlFirmadaAsync(string clave);

    /// <summary>
    /// Elimina el objeto. Acepta una clave ("uploads/x.jpg") o una URL completa
    /// legada del formato público anterior.
    /// </summary>
    Task EliminarArchivoAsync(string claveOUrl);
}
