using System.Diagnostics;

namespace InterviewPrep.Api.Middleware;

/// <summary>
/// Second custom middleware, registered AFTER exception handling, so any exception thrown while
/// timing is still caught by the outer middleware. Pipeline order is a one-way street: a request
/// goes through middlewares top-to-bottom as registered, then the response flows back BOTTOM-to-TOP
/// -- that's why `await _next(context)` splits each middleware's code into a "before" half and an
/// "after" half, and why ordering (auth before authorization, exception handling outermost, etc.)
/// is a real design decision, not boilerplate.
/// </summary>
public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        await _next(context); // "before" code above, "after" code below
        sw.Stop();

        _logger.LogInformation(
            "{Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
            context.Request.Method, context.Request.Path, context.Response.StatusCode, sw.ElapsedMilliseconds);
    }
}
