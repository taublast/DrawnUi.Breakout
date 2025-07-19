using AppoMobi.Specials;
using DrawnUi.Views;
using System.Globalization;

namespace Breakout.Game
{
    public partial class MainPage : BasePageReloadable
    {
        public MainPage()
        {
            Instance = this;
            ApplySelectedLanguage();
        }

        /// <summary>
        /// To change language of this one
        /// </summary>
        public static MainPage Instance;

        /// <summary>
        /// Applies the selected language or detects device language on first start
        /// </summary>
        public void ApplySelectedLanguage()
        {
            if (Preferences.Get("FirstStart", true) ||
                AppSettings.Get(AppSettings.Lang, AppSettings.LangDefault) == string.Empty)
            {
                // detect the device language
                var deviceLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

                if (!MauiProgram.Languages.Contains(deviceLanguage))
                {
                    var fallback = MauiProgram.Languages.First();
                    ApplyLanguage(fallback);
                }
                else
                {
                    ApplyLanguage(deviceLanguage);
                }

                Preferences.Set("FirstStart", false);
            }
            else
            {
                var lang = AppSettings.Get(AppSettings.Lang, AppSettings.LangDefault);
                ApplyLanguage(lang);
            }
        }

        public static void ApplyLanguage(string lang)
        {
            ResStrings.Culture = CultureInfo.CreateSpecificCulture(lang);
            Thread.CurrentThread.CurrentCulture = ResStrings.Culture;
            Thread.CurrentThread.CurrentUICulture = ResStrings.Culture;
            
            AppSettings.Set(AppSettings.Lang, lang);

            switch (lang)
            {
                case "zh":
                    AppFonts.UseGameFont(AppFonts.GameZh, 1.2);
                    break;
                case "ko":
                    AppFonts.UseGameFont(AppFonts.GameKo, 1.1);
                    break;
                default:
                    AppFonts.UseGameFont(AppFonts.Game);
                    break;
            }
        }

        public static void RestartWithLanguage(string lang)
        {
            ApplyLanguage(lang);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                App.Current.MainPage = new NavigationPage(new MainPage());
            });
        }

        static List<KeyValuePair<string, string>> DisplayLanguages;

        public static void SelectAndSetCountry()
        {

            var languages = MauiProgram.Languages;
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
                    // Create picker with detailed camera info
                    var options = DisplayLanguages.Select(c => c.Value).ToArray();
                    var result = await App.Current.MainPage.DisplayActionSheet(ResStrings.Language, ResStrings.BtnCancel, null, options);
                    if (!string.IsNullOrEmpty(result))
                    {
                        var selectedIndex = options.FindIndex(result);
                        if (selectedIndex >= 0)
                        {
                            var lang = languages[selectedIndex];
                            if (lang != AppSettings.Get(AppSettings.Lang, AppSettings.LangDefault))
                            {
                                ApplyLanguage(lang);

                                Super.OnFrame += OnFrame; //will change language on rendering thread too if different from main
                            }
                        }
                    }
                });

                void OnFrame(object o, EventArgs a)
                {
                    Super.OnFrame -= OnFrame;
                    Thread.CurrentThread.CurrentUICulture = ResStrings.Culture;

                    //soft refresh
                    BreakoutGame.Instance.RedrawFromOptions();
                    
                    //mimic HotReload
                    //MainPage.Instance.Build(); // reload game completely
                }

            }



        }


    }
}