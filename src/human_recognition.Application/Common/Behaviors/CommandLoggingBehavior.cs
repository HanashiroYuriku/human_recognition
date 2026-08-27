using System.Diagnostics;
using Cortex.Mediator.Commands;
using Microsoft.Extensions.Logging;

namespace human_recognition.Application.Common.Behaviors;

public class CommandLoggingBehavior<TCommand, TResponse> : ICommandPipelineBehavior<TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    private readonly ILogger<CommandLoggingBehavior<TCommand, TResponse>> _logger;

    public CommandLoggingBehavior(ILogger<CommandLoggingBehavior<TCommand, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TCommand request, CommandHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TCommand).Name;

        // Timer to check execute time
        var timer = Stopwatch.StartNew();

        try
        {
            var response = await next();

            // Success log
            timer.Stop();
            _logger.LogInformation("[END] Execution Request {RequestName} Ended in {Elapsed} ms", requestName, timer.ElapsedMilliseconds);

            return response;
        }
        catch (Exception e)
        {
            // Failed log
            timer.Stop();
            _logger.LogError(e, "[ERROR] {RequestName} Failed in {Elapsed} ms", requestName, timer.ElapsedMilliseconds);
            throw;
        }
    }
}