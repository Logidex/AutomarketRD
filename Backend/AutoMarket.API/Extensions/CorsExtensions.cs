namespace AutoMarket.API.Extensions;

public static class CorsExtensions
{
    public static string AddFrontendCors(this IServiceCollection services, WebApplicationBuilder builder)
    {
        const string frontendPolicy = "FrontendCorsPolicy";

        var originsDeSeccion = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .GetChildren()
            .ToList();

        var allowedOrigins = originsDeSeccion.Count > 0
            ? originsDeSeccion
                .Where(child => !string.IsNullOrWhiteSpace(child.Value))
                .Select(child => child.Value!)
                .ToArray()
            : builder.Configuration["Cors:AllowedOrigins"]?
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                ?? Array.Empty<string>();

        if (builder.Environment.IsDevelopment())
        {
            allowedOrigins = new[]
            {
                "http://localhost:5173",
                "http://127.0.0.1:5173"
            };
        }

        services.AddCors(options =>
        {
            options.AddPolicy(frontendPolicy, policy =>
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return frontendPolicy;
    }
}