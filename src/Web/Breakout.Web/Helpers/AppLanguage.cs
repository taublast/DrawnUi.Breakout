using AppoMobi.Specials;
using Breakout.Game;
using Breakout.Helpers;
using System.Runtime.InteropServices.JavaScript;
using System.Globalization;

namespace Breakout.Resources.Fonts;

public static class AppLanguage
{
    public static List<string> EnabledLanguages = new()
    {
        "en",
        "de",
        "es",
        "fr",
        "it",
        "ru",
        "ja",
        "ko",
        "zh",
    };

    public static void ApplySelected()
    {
        var lang = AppSettings.Get(AppSettings.Lang, AppSettings.LangDefault);
        if (string.IsNullOrWhiteSpace(lang))
        {
            var deviceLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            if (!EnabledLanguages.Contains(deviceLanguage))
            {
                deviceLanguage = EnabledLanguages.First();
            }

            Set(deviceLanguage);
            return;
        }

        Set(lang);
    }

    public static void Set(string lang)
    {
        ResStrings.Culture = CultureInfo.CreateSpecificCulture(lang);

        AppSettings.Set(AppSettings.Lang, lang);

        switch (lang)
        {
            case "zh":
                AppFonts.UseGameFont(AppFonts.GameZh, 1.1);
                break;
            case "ko":
                AppFonts.UseGameFont(AppFonts.GameKo, 1.1);
                break;
            default:
                AppFonts.UseGameFont(AppFonts.Game);
                break;
        }
    }

    public static void SetAndApply(string lang)
    {
        Set(lang);
        BrowserPageInterop.Reload();
    }

    public static void SelectNextAndSet()
    {
        var currentLang = AppSettings.Get(AppSettings.Lang, AppSettings.LangDefault);
        var currentIndex = EnabledLanguages.IndexOf(currentLang);
        if (currentIndex < 0)
        {
            currentIndex = 0;
        }

        var nextIndex = (currentIndex + 1) % EnabledLanguages.Count;
        SetAndApply(EnabledLanguages[nextIndex]);
    }

    public static void SelectAndSet()
    {
        SelectNextAndSet();
    }

    public static string GetCurrentCode()
    {
        var lang = AppSettings.Get(AppSettings.Lang, AppSettings.LangDefault);
        if (string.IsNullOrWhiteSpace(lang))
        {
            lang = EnabledLanguages.First();
        }

        return lang.ToUpperInvariant();
    }

}

internal static partial class BrowserPageInterop
{
    [JSImport("globalThis.location.reload")]
    internal static partial void Reload();
}