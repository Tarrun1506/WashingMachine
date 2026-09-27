namespace WashingMachine.Exceptions;

/// <summary>
/// Represents machine operation errors.
/// </summary>
public class MachineOperationException : Exception
{
    public MachineOperationException(string message)
        : base(message)
    {
    }
}
