using Amazon.S3;
using Amazon.S3.Model;
using AutoMarket.Application.Services;
using Microsoft.Extensions.Configuration;

namespace AutoMarket.Infrastructure.Services;

public class AlmacenadorS3 : IAlmacenadorArchivos
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public AlmacenadorS3(IConfiguration configuration)
    {
        var s3Options = configuration.GetSection("AWS");

        // Inicializamos el cliente con tus llaves de acceso
        _s3Client = new AmazonS3Client(
            s3Options["AccessKey"],
            s3Options["SecretKey"],
            Amazon.RegionEndpoint.GetBySystemName(s3Options["Region"])
        );
        _bucketName = s3Options["BucketName"]!;
    }

    public async Task<string> GuardarArchivoAsync(Stream stream, string nombreArchivo, string contentType)
    {
        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = $"uploads/{nombreArchivo}", // Lo guarda organizadito en una carpeta interna
            InputStream = stream,
            ContentType = contentType
        };

        await _s3Client.PutObjectAsync(putRequest);

        // Retorna la URL pública directa de AWS S3 para que se guarde en tu base de datos PostgreSQL
        return $"https://{_bucketName}.s3.{_s3Client.Config.RegionEndpoint.SystemName}://{nombreArchivo}";
    }

    public async Task EliminarArchivoAsync(string rutaArchivo)
    {
        if (string.IsNullOrWhiteSpace(rutaArchivo)) return;

        // 1. Extraemos solo el nombre del archivo generado (el UUID.jpg)
        var nombreArchivo = rutaArchivo.Split('/').Last();

        // 2. 🌟 LA MAGIA: Reconstruimos la ruta exacta agregando la carpeta de tu bucket
        var keyExacta = $"uploads/{nombreArchivo}";

        var deleteRequest = new Amazon.S3.Model.DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = keyExacta // Le enviamos la ruta completa a S3
        };

        await _s3Client.DeleteObjectAsync(deleteRequest);
    }

}
