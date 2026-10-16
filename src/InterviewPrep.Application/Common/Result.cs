namespace InterviewPrep.Application.Common;

/// <summary>
/// RESULT PATTERN: expected failures (member not found, invalid credentials) are returned as data,
/// not thrown as exceptions. Exceptions are reserved for truly exceptional/unexpected failures
/// (caught centrally by ExceptionHandlingMiddleware in the API). This is a real senior-level talking
/// point: "exceptions for control flow are expensive and hide the failure path from the method
/// signature; Result&lt;T&gt; makes the failure path part of the contract."
/// Loosely mirrors the discriminated-union `Result&lt;T&gt;` approach you'd reach for in TypeScript
/// too (see the React/Angular login pages) -- same idea, different language.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }

    protected Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
}

public sealed class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, string? error) : base(isSuccess, error)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static new Result<T> Failure(string error) => new(false, default, error);
}
