namespace WashingMachine.Models.Common;

/// <summary>
/// Represents the outcome of a service operation.
/// Using a result type instead of exceptions for expected business validation failures
/// keeps the flow clean and avoids exception-for-control-flow anti-pattern.
/// </summary>
public class ServiceResult
{
    public bool Success { get; private init; }
    public string? ErrorMessage { get; private init; }

    private ServiceResult(bool success, string? error)
    {
        Success = success;
        ErrorMessage = error;
    }

    public static ServiceResult Ok() => new(true, null);
    public static ServiceResult Fail(string error) => new(false, error);
}

/// <summary>Generic result carrying a value on success.</summary>
public class ServiceResult<T>
{
    public bool Success { get; private init; }
    public T? Value { get; private init; }
    public string? ErrorMessage { get; private init; }

    private ServiceResult(bool success, T? value, string? error)
    {
        Success = success;
        Value = value;
        ErrorMessage = error;
    }

    public static ServiceResult<T> Ok(T value) => new(true, value, null);
    public static ServiceResult<T> Fail(string error) => new(false, default, error);
}
