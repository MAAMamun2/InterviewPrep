using FluentValidation;
using MediatR;

namespace InterviewPrep.Application.Common.Behaviors;

/// <summary>
/// MEDIATR PIPELINE BEHAVIOR (cross-cutting concern, decorator-ish wrapping of every request):
/// runs every registered FluentValidation validator for TRequest BEFORE the actual handler executes.
/// If any validator fails, the handler never runs at all -- this is the same shape as
/// HishabPOS's real IdempotencyBehavior, just solving a different cross-cutting concern.
/// Order matters: behaviors run in the order they're registered in Program.cs
/// (Logging -> Validation -> Performance -> handler), like middleware in the ASP.NET pipeline.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);
        var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(result => result.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return await next(cancellationToken);
    }
}
