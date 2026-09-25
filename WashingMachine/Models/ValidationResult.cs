namespace WashingMachine.Models;

/// <summary>
/// Represents the result of a validation check.
/// </summary>
public class ValidationResult
{
    /// <summary>Gets whether the validation passed.</summary>
    public bool IsValid { get; private set; }

    /// <summary>Gets the error message if validation failed. Empty if valid.</summary>
    public string ErrorMessage { get; private set; }

    private ValidationResult(bool isValid, string errorMessage)
    {
        IsValid      = isValid;
        ErrorMessage = errorMessage;
    }

    /// <summary>Creates a successful validation result.</summary>
    public static ValidationResult Ok() => new(true, string.Empty);

    /// <summary>Creates a failed validation result with an error message.</summary>
    public static ValidationResult Fail(string errorMessage) => new(false, errorMessage);
}
