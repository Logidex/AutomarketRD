using Microsoft.AspNetCore.Http.Features;

namespace AutoMarket.API.Extensions;

public static class KestrelExtensions
{
    public static void ConfigureUploadLimits(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = 50L * 1024 * 1024;
        });

        builder.Services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 50L * 1024 * 1024;
        });
    }
}
