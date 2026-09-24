namespace WashingMachine.Constants;

/// <summary>
/// Defines application error messages.
/// </summary>
public static class ErrorMessages
{
    /// <summary>
    /// Please enter a valid number.
    /// </summary>
    public const string InvalidNumber = "Please enter a valid number.";

    /// <summary>
    /// Please enter a value greater than zero.
    /// </summary>
    public const string InvalidClothesCount = "Please enter a value greater than zero.";

    /// <summary>
    /// Maximum machine capacity exceeded.
    /// </summary>
    public const string CapacityExceeded = "Maximum machine capacity exceeded.";

    /// <summary>
    /// Cannot remove more clothes than currently loaded.
    /// </summary>
    public const string InvalidClothesRemoval = "Cannot remove more clothes than currently loaded.";

    /// <summary>
    /// Washing cycle is already in progress.
    /// </summary>
    public const string CycleAlreadyRunning = "Washing cycle is already running.";

    /// <summary>
    /// No active washing cycle found.
    /// </summary>
    public const string CycleNotRunning = "No active washing cycle found.";

    /// <summary>
    /// Failed to deserialize data.
    /// </summary>
    public const string DeserializeFailed = "Failed to deserialize data.";

    /// <summary>
    /// Failed to serialize data.
    /// </summary>
    public const string SerializeFailed = "Failed to serialize data.";

    /// <summary>
    /// Failed to read data.
    /// </summary>
    public const string ReadFailed = "Failed to read data.";

    /// <summary>
    /// Failed to write data.
    /// </summary>
    public const string WriteFailed = "Failed to write data.";

    /// <summary>
    /// Access denied while accessing data.
    /// </summary>
    public const string AccessDenied = "Access denied while accessing data.";

    /// <summary>
    /// Invalid file path.
    /// </summary>
    public const string InvalidFilePath = "Invalid file path.";

    /// <summary>
    /// Favourite not found.
    /// </summary>
    public const string FavouriteNotFound = "Favourite not found.";
}