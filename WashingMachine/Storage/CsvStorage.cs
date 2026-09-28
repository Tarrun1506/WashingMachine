using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using WashingMachine.Constants;
using WashingMachine.Exceptions;

namespace WashingMachine.Storage;

/// <summary>
/// Reads and writes flat CSV files through the same interface as JSON storage.
/// Use this as an alternative when the stored records need to be easy to inspect.
/// Nested object values are stored as JSON within their CSV cell.
/// </summary>
public class CsvStorage : IStorage
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    /// <inheritdoc/>
    public async Task<List<T>> LoadAsync<T>(string filePath)
    {
        if (!File.Exists(filePath)) return [];

        try
        {
            using StreamReader reader = new(filePath, Encoding.UTF8);
            string? headerLine = await reader.ReadLineAsync();
            if (headerLine is null) return [];

            string[] headers = ParseRow(headerLine).ToArray();
            List<T> items = [];
            string? line;
            while ((line = await reader.ReadLineAsync()) is not null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] values = ParseRow(line).ToArray();
                items.Add(CreateItem<T>(headers, values));
            }

            return items;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or FormatException or JsonException or NotSupportedException or TargetInvocationException)
        {
            throw new StorageException(ErrorMessages.ReadFailed, exception);
        }
    }

    /// <inheritdoc/>
    public void Save<T>(string filePath, List<T> data) => Write(filePath, data);

    /// <inheritdoc/>
    public T? LoadSingle<T>(string filePath)
    {
        List<T> items = LoadAsync<T>(filePath).GetAwaiter().GetResult();
        return items.Count == 0 ? default : items[0];
    }

    /// <inheritdoc/>
    public void SaveSingle<T>(string filePath, T data) => Write(filePath, data is null ? [] : new List<T> { data });

    private static void Write<T>(string filePath, IEnumerable<T> items)
    {
        try
        {
            PropertyInfo[] properties = GetProperties(typeof(T));
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);

            using StreamWriter writer = new(filePath, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            writer.WriteLine(string.Join(',', properties.Select(property => Escape(property.Name))));
            foreach (T item in items)
            {
                writer.WriteLine(string.Join(',', properties.Select(property => Escape(FormatValue(property.GetValue(item), property.PropertyType)))));
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException or TargetInvocationException)
        {
            throw new StorageException(ErrorMessages.WriteFailed, exception);
        }
    }

    private static T CreateItem<T>(string[] headers, string[] values)
    {
        T item = Activator.CreateInstance<T>();
        Dictionary<string, PropertyInfo> properties = GetProperties(typeof(T))
            .ToDictionary(property => property.Name, StringComparer.OrdinalIgnoreCase);

        for (int index = 0; index < headers.Length && index < values.Length; index++)
        {
            if (!properties.TryGetValue(headers[index], out PropertyInfo? property)) continue;
            property.SetValue(item, ParseValue(values[index], property.PropertyType));
        }

        return item;
    }

    private static PropertyInfo[] GetProperties(Type type) => type
        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Where(property => property.CanRead && property.CanWrite && property.GetIndexParameters().Length == 0)
        .OrderBy(property => property.MetadataToken)
        .ToArray();

    private static string FormatValue(object? value, Type type)
    {
        if (value is null) return string.Empty;
        if (type.IsEnum) return value.ToString() ?? string.Empty;
        if (value is string text) return text;
        if (value is IFormattable formattable && IsSimple(type))
            return formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty;
        return JsonSerializer.Serialize(value, type, JsonOptions);
    }

    private static object? ParseValue(string value, Type type)
    {
        if (string.IsNullOrEmpty(value))
        {
            if (!type.IsValueType || Nullable.GetUnderlyingType(type) is not null) return null;
            return Activator.CreateInstance(type);
        }

        Type targetType = Nullable.GetUnderlyingType(type) ?? type;
        if (targetType == typeof(string)) return value;
        if (targetType.IsEnum) return Enum.Parse(targetType, value, ignoreCase: true);
        if (targetType == typeof(Guid)) return Guid.Parse(value);
        if (targetType == typeof(DateTime)) return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        if (targetType == typeof(DateTimeOffset)) return DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        if (targetType == typeof(TimeSpan)) return TimeSpan.Parse(value, CultureInfo.InvariantCulture);
        if (IsSimple(targetType)) return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        return JsonSerializer.Deserialize(value, targetType, JsonOptions);
    }

    private static bool IsSimple(Type type) => type.IsPrimitive || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(TimeSpan) || type == typeof(Guid);

    private static string Escape(string value) => value.IndexOfAny([',', '"', '\r', '\n']) < 0
        ? value
        : $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";

    private static IEnumerable<string> ParseRow(string row)
    {
        StringBuilder value = new();
        bool inQuotes = false;
        for (int index = 0; index < row.Length; index++)
        {
            char character = row[index];
            if (character == '"')
            {
                if (inQuotes && index + 1 < row.Length && row[index + 1] == '"')
                {
                    value.Append('"');
                    index++;
                }
                else inQuotes = !inQuotes;
            }
            else if (character == ',' && !inQuotes)
            {
                yield return value.ToString();
                value.Clear();
            }
            else value.Append(character);
        }

        if (inQuotes) throw new FormatException("CSV row contains an unclosed quoted field.");
        yield return value.ToString();
    }
}
