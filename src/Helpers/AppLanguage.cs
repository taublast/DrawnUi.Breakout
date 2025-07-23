using AppoMobi.Specials;
using Breakout.Helpers;
using System.Globalization;

namespace Breakout.Resources.Fonts;

public static class AppLanguage
{
    /// <summary>
    /// Enabled languages
    /// </summary>
    public static List<string> EnabledLanguages = new List<string>
    {
        "en", //FIRST is fallback
        "de",
        "es",
        "fr",
        "it",
        "ru",
        "ja",
        "ko",
        "zh",
    };

    /// <summary>
    /// Applies the selected language or detects device language on first start
    /// </summary>
    public static void ApplySelected()
    {
        if (Preferences.Get("FirstStart", true) ||
            AppSettings.Get(AppSettings.Lang, AppSettings.LangDefault) == string.Empty)
        {
            // detect the device language
            var deviceLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            if (!EnabledLanguages.Contains(deviceLanguage))
            {
                var fallback = EnabledLanguages.First();
                Set(fallback);
            }
            else
            {
                Set(deviceLanguage);
            }

            Preferences.Set("FirstStart", false);
        }
        else
        {
            var lang = AppSettings.Get(AppSettings.Lang, AppSettings.LangDefault);
            Set(lang);
        }
    }

    public static void Set(string lang)
    {
        ResStrings.Culture = CultureInfo.CreateSpecificCulture(lang);
        Thread.CurrentThread.CurrentCulture = ResStrings.Culture;
        Thread.CurrentThread.CurrentUICulture = ResStrings.Culture;

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

    public static void RestartWith(string lang)
    {
        Set(lang);

        MainThread.BeginInvokeOnMainThread(() => { App.Current.MainPage = new NavigationPage(new MainPage()); });
    }

    static List<KeyValuePair<string, string>> DisplayLanguages;

    public static void SetAndApply(string lang)
    {
        Set(lang);

        Super.OnFrame +=
            OnFrame; //will change language on rendering thread if different from main
    }

    private static void OnFrame(object o, EventArgs a)
    {
        Super.OnFrame -= OnFrame;
        Thread.CurrentThread.CurrentUICulture = ResStrings.Culture;

        //soft refresh
        BreakoutGame.Instance.RedrawFromOptions();

        //mimic HotReload
        //MainPage.Instance.Build(); // reload game completely as if HotReload hit
    }


    public static void SelectAndSet()
    {
        var languages = EnabledLanguages;
        if (languages.Count() > 2)
        {
            if (DisplayLanguages == null)
            {
                DisplayLanguages = new();
                foreach (var lang in languages)
                {
                    var culture = CultureInfo.CreateSpecificCulture(lang);
                    var title = culture.NativeName.ToTitleCase().ToTitleCase("(");
                    DisplayLanguages.Add(new(lang, title));
                }
            }

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var options = DisplayLanguages.Select(c => c.Value).ToArray();
                
                //block game input from keys
                BreakoutGame.Instance.Pause();

                var result =
                    await App.Current.MainPage.DisplayActionSheet(ResStrings.Language, ResStrings.BtnCancel, null,
                        options);

                BreakoutGame.Instance.Resume();

                if (!string.IsNullOrEmpty(result))
                {
                    var selectedIndex = options.FindIndex(result);
                    if (selectedIndex >= 0)
                    {
                        var lang = languages[selectedIndex];
                        if (lang != AppSettings.Get(AppSettings.Lang, AppSettings.LangDefault))
                        {
                            SetAndApply(lang);
                        }
                    }
                }
            });

        }
    }
}