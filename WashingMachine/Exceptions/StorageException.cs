namespace WashingMachine.Exceptions;

/// <summary>
/// Represents storage related exceptions.
/// </summary>
public class StorageException : Exception
{
    public StorageException(string message)
        : base(message)
    {
    }

    public StorageException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}