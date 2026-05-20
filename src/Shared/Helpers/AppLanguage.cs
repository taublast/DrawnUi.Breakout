using AppoMobi.Specials;
using Breakout.Game;
using Breakout.Game.Controls;
using Breakout.Helpers;
using System.Globalization;

namespace Breakout.Resources.Fonts;

public static partial class AppLanguage
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

    public static void Set(string lang)
    {
        ResStrings.Culture = CultureInfo.CreateSpecificCulture(lang);

#if !BROWSER
        Thread.CurrentThread.CurrentCulture = ResStrings.Culture;
        Thread.CurrentThread.CurrentUICulture = ResStrings.Culture;
#endif

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
}