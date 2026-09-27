namespace WashingMachine.Constants;

/// <summary>
/// Defines application error messages.
/// </summary>
public static class ErrorMessages
{
    public const string InvalidClothesCount = "Please enter a value greater than zero.";
    public const string CapacityExceeded = "Maximum machine capacity exceeded.";
    public const string MachineFull = "The machine is already at full capacity.";
    public const string InvalidClothesRemoval = "Cannot remove more clothes than currently loaded.";
    public const string NoClothesToRemove = "There are no clothes to remove.";
    public const string ClothesRequiredToStart = "Please add clothes before starting the cycle.";
    public const string CycleAlreadyRunning = "Washing cycle is already running.";
    public const string CycleNotRunning = "No active washing cycle found.";
    public const string CycleIsPaused = "A cycle is paused. Resume it instead of starting a new one.";
    public const string MachineNotPaused = "Machine is not paused.";
    public const string CannotModifyWhileRunning = "Cannot change settings while the machine is running. Pause the cycle first.";
    public const string CannotAddWhileRunning = "Cannot add clothes while the machine is running. Please pause the cycle first.";
    public const string CannotRemoveWhileRunning = "Cannot remove clothes while the machine is running. Please pause the cycle first.";
    public const string InvalidProgram = "Invalid program. Choose Cotton, Quick Wash, Synthetic, Wool, or Heavy Wash.";
    public const string InvalidTemperature = "Invalid temperature. Enter 0=Cold, 1=Warm, or 2=Hot.";
    public const string InvalidWaterLevel = "Invalid water level. Enter 0=Low, 1=Medium, or 2=High.";
    public const string InvalidSpinSpeed = "Invalid spin speed. Enter 1=400, 2=800, 3=1000, 4=1200, or 5=1400 rpm.";
    public const string InvalidYesNo = "Please enter y or n.";
    public const string InvalidMenuChoice = "Invalid choice. Please try again.";
    public const string InvalidFavouriteSelection = "Invalid selection. Enter a number from the list.";
    public const string FavouriteNameRequired = "Favourite name cannot be empty.";
    public const string FavouriteNameTooLong = "Favourite name must be 30 characters or fewer.";
    public const string DuplicateFavouriteName = "A favourite with this name already exists.";
    public const string FavouriteNotFound = "Favourite not found.";
    public const string NoFavouritesSaved = "No favourites saved yet.";
    public const string SettingsRequired = "Wash settings are required.";
    public const string InvalidRestorationChoice = "Invalid choice. Enter 1 to continue or 2 to reset.";
    public const string DeserializeFailed = "Failed to deserialize data.";
    public const string SerializeFailed = "Failed to serialize data.";
    public const string ReadFailed = "Failed to read data.";
    public const string WriteFailed = "Failed to write data.";
    public const string AccessDenied = "Access denied while accessing data.";
    public const string InvalidFilePath = "Invalid file path.";
}
