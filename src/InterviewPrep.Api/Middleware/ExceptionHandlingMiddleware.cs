using System.Net;
using FluentValidation;
using InterviewPrep.Application.Common.Exceptions;

namespace InterviewPrep.Api.Middleware;

/// <summary>
/// CUSTOM MIDDLEWARE: every request flows through this BEFORE anything else (registered first in
/// Program.cs, outermost in the pipeline) by calling `await _next(context)` and catching whatever
/// the rest of the pipeline throws. This is the textbook middleware shape: constructor takes
/// RequestDelegate next, InvokeAsync calls it, logic wraps around that call. Centralizing error
/// mapping here means controllers never write try/catch for these cases.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.BadRequest, "Validation failed",
                ex.Errors.Select(e => e.ErrorMessage));
        }
        catch (NotFoundException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.NotFound, "Not found", new[] { ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.Unauthorized, "Unauthorized", new[] { ex.Message });
        }
        catch (Exception ex)
        {
            // Never leak stack traces/internal details to the client -- log the real exception,
            // return a generic message. Leaking internals here is its own small security issue
            // (information disclosure) worth naming if asked.
            _logger.LogError(ex, "Unhandled exception");
            await WriteProblemAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred",
                Array.Empty<string>());
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, HttpStatusCode status, string title, IEnumerable<string> errors)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)status;

        var problem = new
        {
            title,
            status = (int)status,
            errors
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}
