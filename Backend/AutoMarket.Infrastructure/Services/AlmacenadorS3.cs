using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using AutoMarket.Application.Services;
using Microsoft.Extensions.Configuration;

namespace AutoMarket.Infrastructure.Services;

public class AlmacenadorS3 : IAlmacenadorArchivos
{
    private const string PREFIJO_OBJECT_KEY = "uploads/";

    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _region;
    private readonly bool _esAws;
    private readonly TimeSpan _expiracionUrls;

    public AlmacenadorS3(IConfiguration configuration)
    {
        var s3Options = configuration.GetSection("AWS");

        _region = s3Options["Region"]!;
        _bucketName = s3Options["BucketName"]!;

        var expiracionMinutos = s3Options.GetValue("ExpirationMinutes", 15);
        _expiracionUrls = TimeSpan.FromMinutes(expiracionMinutos);

        var serviceUrl = s3Options["ServiceUrl"];

        // R2 de Cloudflare (y otros servicios compatibles con S3) no soporta el
        // cifrado del lado del servidor indicado por el cliente: cifra en reposo.
        _esAws = string.IsNullOrWhiteSpace(serviceUrl) ||
                 serviceUrl.Contains("amazonaws", StringComparison.OrdinalIgnoreCase);

        var config = new AmazonS3Config
        {
            ServiceURL = serviceUrl,
            ForcePathStyle = true,
            // R2 (y otros servicios compatibles con S3) no implementa la firma
            // chunked con trailer (STREAMING-AWS4-HMAC-SHA256-PAYLOAD-TRAILER)
            // que el SDK v4 usa por defecto al calcular checksum en streaming.
            RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED
        };

        // AWS real (sin ServiceUrl) requiere RegionEndpoint; con R2 basta ServiceURL.
        if (_esAws && !string.IsNullOrWhiteSpace(_region))
        {
            config.RegionEndpoint = RegionEndpoint.GetBySystemName(_region);
        }

        _s3Client = new AmazonS3Client(
            s3Options["AccessKey"],
            s3Options["SecretKey"],
            config
        );
    }

    /// <summary>
    /// Guarda un archivo en el bucket S3 privada con encriptacion AES256.
    /// Devuelve la clave (ruta) del objeto almacenado, que se usara para generar URLs firmadas.
    /// </summary>
    /// <param name="stream">Stream del archivo a almacenar.</param>
    /// <param name="nombreArchivo">Nombre del archivo (se anadira el prefijo 'uploads/' automatico).</param>
    /// <param name="contentType">Tipo MIME del archivo (ej. image/png, image/jpeg).</param>
    /// <returns>Clave S3 bajo la cual el archivo fue almacenado (ej. 'uploads/foto.jpg').</returns>
    public async Task<string> GuardarArchivoAsync(Stream stream, string nombreArchivo, string contentType)
    {
        var key = $"{PREFIJO_OBJECT_KEY}{nombreArchivo}";

        // R2 no implementa la subida chunked del SDK (STREAMING-AWS4-HMAC-SHA256-PAYLOAD[-TRAILER]).
        // Se desactiva el chunking y se bufferiza el stream (longitud conocida) para que el SDK
        // firme el payload completo con la firma estándar (AWS4-HMAC-SHA256-PAYLOAD).
        using var memoria = new MemoryStream();
        await stream.CopyToAsync(memoria);
        memoria.Position = 0;

        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = memoria,
            ContentType = contentType,
            UseChunkEncoding = false
        };

        if (_esAws)
        {
            putRequest.ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256;
        }

        await _s3Client.PutObjectAsync(putRequest);

        return key;
    }

    /// <summary>
    /// Genera una URL firmada de S3 con duracion de expiracion configurada (por defecto 15 minutos).
    /// La URL permite descargar o visualizar el archivo privado sin necesidad de credenciales AWS.
    /// </summary>
    /// <param name="clave">Clave S3 del archivo (ruta dentro del bucket, ej. 'uploads/foto.jpg').</param>
    /// <returns>URL firmada de solo uso (HTTPS) que vencera despues del periodo de expiracion.</returns>
    public Task<string> GenerarUrlFirmadaAsync(string clave)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = NormalizarClave(clave),
            Expires = DateTime.UtcNow.Add(_expiracionUrls),
            Protocol = Protocol.HTTPS
        };

        return Task.FromResult(_s3Client.GetPreSignedURL(request));
    }

    /// <summary>
    /// Elimina un archivo del bucket S3 utilizando su clave (ruta).
    /// Si la clave proporcionada no tiene un formato valido, el metodo la normaliza automaticamente.
    /// </summary>
    /// <param name="claveOUrl">Clave S3 o URL completa del archivo a eliminar.</param>
    public Task EliminarArchivoAsync(string claveOUrl)
    {
        if (string.IsNullOrWhiteSpace(claveOUrl)) return Task.CompletedTask;

        var key = NormalizarClave(claveOUrl);

        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        return _s3Client.DeleteObjectAsync(deleteRequest);
    }

    /// <summary>
    /// Convierte una URL pública legada ("https://bucket.s3.region.amazonaws.com/uploads/x.jpg")
    /// o una clave ("uploads/x.jpg") en la clave de objeto correspondiente.
    /// </summary>
    private static string NormalizarClave(string claveOUrl)
    {
        var candidato = claveOUrl.Trim();

        if (Uri.TryCreate(candidato, UriKind.Absolute, out var uri) &&
            !string.IsNullOrEmpty(uri.Host))
        {
            var path = uri.AbsolutePath.TrimStart('/');
            return path.StartsWith(PREFIJO_OBJECT_KEY, StringComparison.OrdinalIgnoreCase)
                ? path
                : $"{PREFIJO_OBJECT_KEY}{path.TrimStart('/')}";
        }

        return candidato;
    }
}
