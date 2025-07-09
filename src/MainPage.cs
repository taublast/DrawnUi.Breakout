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

        public void ApplySelectedLanguage()
        {
            if (Preferences.Get("FirstStart", true))
            {
                if (!MauiProgram.Languages.Contains(Preferences.Get("DeviceLang", "")))
                {
                    var fallback = MauiProgram.Languages.First();
                    ApplyLanguage(fallback); // will set SelectedLang
                }
            }

            ApplyLanguage(Preferences.Get("SelectedLang", ""));
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

        Canvas Canvas;

        public override void Build()
        {
            Canvas?.Dispose();

            Canvas = new Canvas()
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                Gestures = GesturesMode.Lock,
                RenderingMode = RenderingModeType.Accelerated,
                BackgroundColor = Colors.Black,

                Content = new SkiaLayer()
                {
                    Children =
                    {
                        new SkiaViewSwitcher()
                        {
                            HorizontalOptions = LayoutOptions.Fill,
                            VerticalOptions = LayoutOptions.Fill,
                            SelectedIndex = 0,
                            Children =
                            {
                                new Game.BreakoutGame(),
                            }
                        }.Assign(out ViewsContainer),

                        new SkiaLabelFps()
                        {
                            Margin = new(0, 0, 4, 24),
                            VerticalOptions = LayoutOptions.End,
                            HorizontalOptions = LayoutOptions.End,
                            Rotation = -45,
                            FontSize = 11,
                            BackgroundColor = Colors.DarkRed,
                            TextColor = Colors.White,
                            ZIndex = 110,
                        }
                    }
                }.Fill()
            };

            this.Content = Canvas;
        }


    }



}