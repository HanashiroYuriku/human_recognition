using System.Diagnostics;
using Cortex.Mediator.Queries;
using Microsoft.Extensions.Logging;

namespace human_recognition.Application.Common.Behaviors;

public class QueryLoggingBehavior<TQuery, TResponse> : IQueryPipelineBehavior<TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    private readonly ILogger<QueryLoggingBehavior<TQuery, TResponse>> _logger;

    public QueryLoggingBehavior(ILogger<QueryLoggingBehavior<TQuery, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TQuery request, QueryHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TQuery).Name;

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