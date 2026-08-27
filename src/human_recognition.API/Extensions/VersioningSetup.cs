using Asp.Versioning;

namespace human_recognition.Api.Extensions;

public static class VersioningSetup
{
    public static IServiceCollection AddVersioningConfiguration(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            // Default version (v 1.0)
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;

            // Send header respons to client about available API varsion
            options.ReportApiVersions = true;

            // Read version from URL (e.g.,: /api/v1/users)
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            // Format version for Swagger ('v'VVV = v1, v2, dst)
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}