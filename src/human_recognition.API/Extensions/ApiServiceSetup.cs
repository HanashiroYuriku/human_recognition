using System.Text.Json;
using System.Text.Json.Serialization;
using human_recognition.Api.Handlers;
using human_recognition.Api.Middlewares;

namespace human_recognition.Api.Extensions;

public static class ApiServicesExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Controller & JSON Options
        services.AddControllers()
            .AddJsonOptions(opt =>
            {
                opt.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        // 2. Exception Handling
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // 3. Security & Authentication
        services.AddSingleton<JwtMiddleware>();
        services.AddJwtAuthentication(configuration);
        services.AddCorsConfiguration(configuration);

        // 4. API Docs
        services.AddEndpointsApiExplorer();
        services.AddSwaggerConfiguration();
        services.AddVersioningConfiguration();

        return services;
    }
}