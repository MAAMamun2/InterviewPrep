using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InterviewPrep.Application.Common.Behaviors;

/// <summary>
/// Flags any request that takes longer than 500ms. This is the kind of cross-cutting concern
/// that's easy to bolt on with MediatR behaviors and painful to retrofit into every handler
/// individually -- a good answer to "why use a mediator/pipeline instead of calling services directly?"
/// </summary>
public sealed class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private const int SlowThresholdMs = 500;
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var response = await next(cancellationToken);
        sw.Stop();

        if (sw.ElapsedMilliseconds > SlowThresholdMs)
        {
            _logger.LogWarning(
                "Slow request: {RequestName} took {ElapsedMs}ms",
                typeof(TRequest).Name, sw.ElapsedMilliseconds);
        }

        return response;
    }
}
