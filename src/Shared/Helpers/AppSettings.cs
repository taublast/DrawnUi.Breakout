using System.Text.Json;
#if BROWSER
using System.Runtime.InteropServices.JavaScript;
using DrawnUi.Views;
#endif

namespace Breakout.Helpers;

public static class AppSettings
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static readonly string Lang = "lang";
    public static readonly string LangDefault = string.Empty;

    public static readonly string AppLaunchCount = "runs";
    public static readonly int AppLaunchCountDefault = 0;

    public static readonly string AppBootstrapVersion = "boot";
    public static readonly int AppBootstrapVersionDefault = 0;
    public const int CurrentBootstrapVersion = 1;

    public static readonly string MusicOn = "mus";
    public static readonly bool MusicOnDefault = true;

    public static readonly string SoundsOn = "fx";
    public static readonly bool SoundsOnDefault = true;

    public static readonly string InputPressEnabled = "press";
    public static readonly bool InputPressEnabledDefault = false;

    public static bool Has(string key)
    {
#if BROWSER
        if (_settings.ContainsKey(key))
            return true;

        try
        {
            return !string.IsNullOrEmpty(BrowserStorageInterop.Get(GetBrowserKey(key)));
        }
        catch
        {
            return false;
        }
#elif DESKTOP
        lock (DesktopSettingsSync)
        {
            return GetDesktopSettings().ContainsKey(key);
        }
#else
        return Preferences.Default.ContainsKey(key);
#endif
    }

    public static bool GetInitialInputPressEnabledDefault()
    {
#if BROWSER
        return BrowserApi.IsMobileBrowser();
#else
        return InputPressEnabledDefault;
#endif
    }

    public static void ApplyStartupBootstrapIfNeeded()
    {
        var bootstrapVersion = Get(AppBootstrapVersion, AppBootstrapVersionDefault);
        if (bootstrapVersion >= CurrentBootstrapVersion)
            return;

        if (!Has(InputPressEnabled))
        {
            Set(InputPressEnabled, GetInitialInputPressEnabledDefault());
        }

        Set(AppBootstrapVersion, CurrentBootstrapVersion);
    }

#if BROWSER
    private static readonly Dictionary<string, object> _settings = new();

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
#elif DESKTOP
    private static readonly object DesktopSettingsSync = new();
    private static Dictionary<string, string>? _desktopSettings;

    public static T Get<T>(string key, T defaultValue)
    {
        lock (DesktopSettingsSync)
        {
            if (GetDesktopSettings().TryGetValue(key, out var serializedValue))
            {
                try
                {
                    var deserialized = JsonSerializer.Deserialize<T>(serializedValue, JsonOptions);
                    if (deserialized is not null)
                    {
                        return deserialized;
                    }
                }
                catch
                {
                }
            }

            return defaultValue;
        }
    }

    public static void Set<T>(string key, T value)
    {
        lock (DesktopSettingsSync)
        {
            var settings = GetDesktopSettings();
            settings[key] = JsonSerializer.Serialize(value, JsonOptions);
            SaveDesktopSettings(settings);
        }
    }

    private static Dictionary<string, string> GetDesktopSettings()
    {
        if (_desktopSettings != null)
            return _desktopSettings;

        if (!File.Exists(DesktopSettingsPath))
        {
            _desktopSettings = new();
            return _desktopSettings;
        }

        try
        {
            var json = File.ReadAllText(DesktopSettingsPath);
            _desktopSettings = JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions) ?? new();
        }
        catch
        {
            _desktopSettings = new();
        }

        return _desktopSettings;
    }

    private static void SaveDesktopSettings(Dictionary<string, string> settings)
    {
        var settingsDirectory = Path.GetDirectoryName(DesktopSettingsPath);
        if (!string.IsNullOrEmpty(settingsDirectory))
        {
            Directory.CreateDirectory(settingsDirectory);
        }

        File.WriteAllText(DesktopSettingsPath, JsonSerializer.Serialize(settings, JsonOptions));
    }

    private static string DesktopSettingsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DrawnUi.Breakout",
        "settings.json");
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
