namespace AutoMarket.Application.Services;

public interface IAlmacenadorArchivos
{
    Task<string> GuardarArchivoAsync(Stream stream, string nombreArchivo, string contentType);
    Task EliminarArchivoAsync(string rutaArchivo);
}
