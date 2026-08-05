using Amazon.S3;
using Amazon.S3.Model;
using AutoMarket.Application.Services;
using Microsoft.Extensions.Configuration;

namespace AutoMarket.Infrastructure.Services;

public class AlmacenadorS3 : IAlmacenadorArchivos
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _region;

    public AlmacenadorS3(IConfiguration configuration)
    {
        var s3Options = configuration.GetSection("AWS");

        _region = s3Options["Region"]!;
        _bucketName = s3Options["BucketName"]!;

        _s3Client = new AmazonS3Client(
            s3Options["AccessKey"],
            s3Options["SecretKey"],
            Amazon.RegionEndpoint.GetBySystemName(_region)
        );
    }

    public async Task<string> GuardarArchivoAsync(Stream stream, string nombreArchivo, string contentType)
    {
        var key = $"uploads/{nombreArchivo}";

        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = stream,
            ContentType = contentType
        };

        await _s3Client.PutObjectAsync(putRequest);

        return $"https://{_bucketName}.s3.{_region}.amazonaws.com/{key}";
    }

    public async Task EliminarArchivoAsync(string rutaArchivo)
    {
        if (string.IsNullOrWhiteSpace(rutaArchivo)) return;

        var uri = new Uri(rutaArchivo);
        var key = uri.AbsolutePath.TrimStart('/');

        if (string.IsNullOrWhiteSpace(key)) return;

        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        await _s3Client.DeleteObjectAsync(deleteRequest);
    }
}
