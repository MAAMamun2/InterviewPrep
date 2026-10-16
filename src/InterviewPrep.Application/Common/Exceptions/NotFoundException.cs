namespace InterviewPrep.Application.Common.Exceptions;

/// <summary>Thrown for the one genuinely "shouldn't happen on the happy path" case, caught centrally
/// by ExceptionHandlingMiddleware and mapped to HTTP 404. Everything else expected (bad login, etc.)
/// uses Result&lt;T&gt; instead -- see Common/Result.cs for why.</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.")
    {
    }
}
