using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace human_recognition.Api.Extensions;

public static class HealthCheckSetup
{
    // Gunakan ekstensi untuk WebApplication (atau IEndpointRouteBuilder)
    public static void MapCustomHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/api/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Add other new heatlh check endpoint here
        // app.MapHealthChecks("/api/health/live", new HealthCheckOptions { Predicate = _ => false });
    }
}