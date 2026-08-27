namespace human_recognition.Api.Extensions;

public static class CorsSetup
{
    public const string PolicyName = "human_recognitionCORSPolicy";

    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        services.AddCors(opt =>
        {
            opt.AddPolicy(PolicyName, builder =>
            {
                builder
                    .WithOrigins(allowedOrigins) // your allowed port. you can registry your allowed port on appsettings.json
                    .AllowAnyHeader() // allow all header type
                    .AllowAnyMethod() // allow all HTTP method
                    .AllowCredentials(); // allow to send refresh token by httpOnly Cookies from Front End
            });
        });

        return services;
    }
}