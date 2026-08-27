using System.Text.Json;
using Cortex.Mediator.Commands;
using FluentValidation;

namespace human_recognition.Application.Common.Behaviors;

public class ValidationBehavior<TCommand, TResponse> : ICommandPipelineBehavior<TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    private readonly IEnumerable<IValidator<TCommand>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TCommand>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TCommand request, CommandHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // if have no validator -> continue to handler
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TCommand>(request);

        // run all validattor
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        // retrive all errors
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // set to camel case
        foreach (var failure in failures)
        {
            failure.PropertyName = JsonNamingPolicy.CamelCase.ConvertName(failure.PropertyName);
        }

        // throw if error
        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}