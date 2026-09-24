namespace WashingMachine.Exceptions;

/// <summary>Thrown when a machine operation is not valid in the current state.</summary>
public class MachineOperationException : Exception
{
    public MachineOperationException(string message) : base(message) { }
    public MachineOperationException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>Thrown when a storage operation (file read/write/serialization) fails.</summary>
public class StorageException : Exception
{
    public StorageException(string message) : base(message) { }
    public StorageException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>Thrown when an operation is attempted from an invalid machine state.</summary>
public class InvalidMachineStateException : MachineOperationException
{
    public InvalidMachineStateException(string message) : base(message) { }
}
