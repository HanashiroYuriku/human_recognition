using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;

namespace human_recognition.Api.Middlewares;

public class JwtMiddleware : JwtBearerEvents
{
    public override async Task Challenge(JwtBearerChallengeContext ctx)
    {
        ctx.HandleResponse();
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        ctx.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Title = "Unauthorized",
            Type = "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1",
            Status = StatusCodes.Status401Unauthorized,
            Detail = "Authentication failed. Token is missing or invalid.",
            Instance = ctx.Request.Path
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        };

        await ctx.Response.WriteAsJsonAsync(problemDetails, jsonOptions);
    }

    public override async Task Forbidden(ForbiddenContext ctx)
    {
        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
        ctx.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Title = "Forbidden",
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3",
            Status = StatusCodes.Status403Forbidden,
            Detail = "You don't have permission to access this resource.",
            Instance = ctx.Request.Path
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        };

        await ctx.Response.WriteAsJsonAsync(problemDetails, jsonOptions);
    }
}