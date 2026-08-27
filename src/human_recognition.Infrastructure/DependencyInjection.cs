using human_recognition.Application.Common.Interfaces.Authentication;
using human_recognition.Application.Common.Interfaces.PersonDetection;
using human_recognition.Application.Common.Interfaces.Repositories;
using human_recognition.Infrastructure.Authentication;
using human_recognition.Infrastructure.Data;
using human_recognition.Infrastructure.Data.Repositories;
using human_recognition.Infrastructure.ExternalServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace human_recognition.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // === Pomelo MySQL
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        // register Pomelo
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        });

        // register Password Hash
        services.AddScoped<IPasswordHasher, BycryptPasswordHasher>();
        // register user repository
        services.AddScoped<IUserRepository, UserRepository>();
        // register JWT Settings
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        // register JWT Generator
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        // register Database Transaction Manager
        services.AddScoped<IDbTransactionManager, DbTransactionManager>();

        string modelPath = configuration.GetValue<string>("AI:YoloxModelPath")
            ?? "D:\\Kantor\\Project\\Temp\\human_recognition\\models\\yolo11m.onnx";

        services.AddSingleton<IPersonDetector>(provider => new YoloxDetector(modelPath));

        return services;
    }
}