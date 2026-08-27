using human_recognition.Api.Extensions;
using human_recognition.Api.Middlewares;
using human_recognition.Application;
using human_recognition.Infrastructure;
using Serilog;

// Setup Serilog Logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console().CreateBootstrapLogger();

try
{
    Log.Information("[START] Start human_recognition API");
    var builder = WebApplication.CreateBuilder(args);
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    // Serilog Extension
    builder.AddSerilogConfiguration();

    // Add services/application layer
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // Add api layer
    builder.Services.AddApiServices(builder.Configuration);

    builder.Services.AddHealthChecks()
        .AddMySql(connectionString!, name: "database");

    // Build builder
    var app = builder.Build();

    // Configure HTTP Pipeline
    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        app.UseHttpsRedirection();
    }

    app.UseCors(CorsSetup.PolicyName);

    app.UseAuthentication();
    app.UseMiddleware<RequestLoggingMiddleware>();
    app.UseAuthorization();

    app.MapControllers();
    app.MapCustomHealthChecks();

    app.Lifetime.ApplicationStarted.Register(() =>
    {
        Log.Information("human_recognition API is running on port: {Urls}", string.Join(", ", app.Urls));
    });

    app.Run();
}
catch (HostAbortedException) { }
catch (Exception e)
{
    Log.Fatal("[ERROR] Failed to start server: {err}", e);
}
finally
{
    Log.CloseAndFlush();
}