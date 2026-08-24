using AutoMarket.Application.Services;

namespace AutoMarket.API.Fakes;

/// <summary>
/// Almacenador de archivos en disco local para desarrollo local y E2E:
/// guarda en wwwroot/e2e-archivos y devuelve claves "e2e/xxx.ext". Las
/// "URLs firmadas" son rutas estáticas (/e2e-archivos/xxx.ext) servidas por
/// UseStaticFiles, que solo se habilita en Development.
///
/// Se registra SOLO cuando el entorno es Development y AWS:AccessKey es
/// "dummy" (docker-compose.e2e.yml / local sin nube). Staging y producción
/// siempre usan el AlmacenadorS3 real.
/// </summary>
public class AlmacenadorArchivosLocal : IAlmacenadorArchivos
{
    public const string PREFIJO_CLAVE = "e2e/";

    private readonly string _raiz;

    public AlmacenadorArchivosLocal(IHostEnvironment env)
    {
        _raiz = Path.Combine(env.ContentRootPath, "wwwroot", "e2e-archivos");
        Directory.CreateDirectory(_raiz);
    }

    public async Task<string> GuardarArchivoAsync(Stream stream, string nombreArchivo, string contentType)
    {
        var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(nombreArchivo)}";
        var ruta = Path.Combine(_raiz, fileName);

        await using var destino = File.Create(ruta);
        await stream.CopyToAsync(destino);

        return $"{PREFIJO_CLAVE}{fileName}";
    }

    public Task<string> GenerarUrlFirmadaAsync(string clave)
    {
        var fileName = Path.GetFileName(clave.Replace('\\', '/'));
        return Task.FromResult($"/e2e-archivos/{fileName}");
    }

    public Task EliminarArchivoAsync(string claveOUrl)
    {
        var clave = claveOUrl.Replace('\\', '/');
        var indice = clave.IndexOf(PREFIJO_CLAVE, StringComparison.Ordinal);

        if (indice < 0)
        {
            return Task.CompletedTask;
        }

        var fileName = Path.GetFileName(clave[(indice + PREFIJO_CLAVE.Length)..]);
        var ruta = Path.Combine(_raiz, fileName);

        if (File.Exists(ruta))
        {
            File.Delete(ruta);
        }

        return Task.CompletedTask;
    }
}
