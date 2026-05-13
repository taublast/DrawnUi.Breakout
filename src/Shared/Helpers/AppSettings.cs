using System.Text.Json;
#if BROWSER
using System.Runtime.InteropServices.JavaScript;
#endif

namespace Breakout.Helpers;

public static class AppSettings
{
    public static readonly string Lang = "lang";
    public static readonly string LangDefault = string.Empty;

    public static readonly string MusicOn = "mus";
    public static readonly bool MusicOnDefault = true;

    public static readonly string SoundsOn = "fx";
    public static readonly bool SoundsOnDefault = true;

    public static readonly string InputPressEnabled = "press";
    public static readonly bool InputPressEnabledDefault = false;

#if BROWSER
    private static readonly Dictionary<string, object> _settings = new();
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static T Get<T>(string key, T defaultValue)
    {
        if (_settings.TryGetValue(key, out var value) && value is T typed)
            return typed;

        try
        {
            var storedValue = BrowserStorageInterop.Get(GetBrowserKey(key));
            if (string.IsNullOrEmpty(storedValue))
                return defaultValue;

            var deserialized = JsonSerializer.Deserialize<T>(storedValue, JsonOptions);
            if (deserialized is null)
                return defaultValue;

            _settings[key] = deserialized!;
            return deserialized;
        }
        catch
        {
            return defaultValue;
        }
    }

    public static void Set<T>(string key, T value)
    {
        _settings[key] = value!;

        try
        {
            BrowserStorageInterop.Set(GetBrowserKey(key), JsonSerializer.Serialize(value, JsonOptions));
        }
        catch
        {
            // Ignore browser storage failures and keep the in-memory fallback.
        }
    }

    private static string GetBrowserKey(string key) => $"breakout.{key}";
#else
    public static T Get<T>(string key, T defaultValue)
    {
        return Preferences.Default.Get(key, defaultValue);
    }

    public static void Set<T>(string key, T value)
    {
        Preferences.Default.Set(key, value);
    }
#endif
}

#if BROWSER
internal static partial class BrowserStorageInterop
{
    [JSImport("globalThis.breakoutStorage.get")]
    internal static partial string? Get(string key);

    [JSImport("globalThis.breakoutStorage.set")]
    internal static partial void Set(string key, string value);
}
#endif
