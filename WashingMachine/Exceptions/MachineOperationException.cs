namespace WashingMachine.Exceptions;

/// <summary>
/// Represents machine operation errors.
/// </summary>
public class MachineOperationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MachineOperationException"/> class.
    /// </summary>
    /// <param name="message">Exception message.</param>
    public MachineOperationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MachineOperationException"/> class.
    /// </summary>
    /// <param name="message">Exception message.</param>
    /// <param name="innerException">Inner exception.</param>
    public MachineOperationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}