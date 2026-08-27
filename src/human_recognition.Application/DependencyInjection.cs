using human_recognition.Application.Common.Behaviors;
using Cortex.Mediator.Commands;
using Cortex.Mediator.DependencyInjection;
using Cortex.Mediator.Queries;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace human_recognition.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // register all validators from Fluent Validation automaticly
        services.AddValidatorsFromAssembly(assembly);

        // register cortex
        services.AddCortexMediator([typeof(DependencyInjection)]);

        // logging command cortex
        services.AddTransient(typeof(ICommandPipelineBehavior<,>), typeof(CommandLoggingBehavior<,>));
        // validation command
        services.AddTransient(typeof(ICommandPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        // logging query cortex
        services.AddTransient(typeof(IQueryPipelineBehavior<,>), typeof(QueryLoggingBehavior<,>));

        return services;
    }
}