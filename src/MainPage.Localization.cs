using Breakout.Game;
using DrawnUi.Controls;
using DrawnUi.Views;
using PreviewFramework;
using System.Globalization;
using Breakout;

namespace Breakout.Game
{
    public partial class MainPage : BasePageReloadable
    {
        public MainPage()
        {
            ApplySelectedLanguage();
        }

        /// <summary>
        /// Applies the selected language or detects device language on first start
        /// </summary>
        public void ApplySelectedLanguage()
        {
            if (Preferences.Get("FirstStart", true))
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
                ApplyLanguage(Preferences.Get("SelectedLang", ""));
            }
        }

        public void ApplyLanguage(string lang)
        {
            ResStrings.Culture = CultureInfo.CreateSpecificCulture(lang);
            Thread.CurrentThread.CurrentCulture = ResStrings.Culture;
            Thread.CurrentThread.CurrentUICulture = ResStrings.Culture;
            Preferences.Set("DeviceLang", lang);
            Preferences.Set("SelectedLang", lang);
        }

        public static void RestartWithLanguage(string lang)
        {
            ResStrings.Culture = CultureInfo.CreateSpecificCulture(lang);

            //whatever thread we are in
            Thread.CurrentThread.CurrentCulture = ResStrings.Culture;
            Thread.CurrentThread.CurrentUICulture = ResStrings.Culture;
            Preferences.Set("DeviceLang", lang);
            Preferences.Set("SelectedLang", lang);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                App.Current.MainPage = new NavigationPage(new MainPage());
            });
        }

    }
}