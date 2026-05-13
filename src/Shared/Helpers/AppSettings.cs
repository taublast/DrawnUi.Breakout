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
    private static Dictionary<string, object> _settings = new();

    public static T Get<T>(string key, T defaultValue)
    {
        if (_settings.TryGetValue(key, out var value) && value is T typed)
            return typed;
        return defaultValue;
    }

    public static void Set<T>(string key, T value)
    {
        _settings[key] = value!;
    }
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
