using AppoMobi.Specials;
using Breakout.Game;
using Breakout.Game.Controls;
using Breakout.Helpers;
using System.Globalization;

namespace Breakout.Resources.Fonts;

public static partial class AppLanguage
{
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

    public static void SetAndApply(string lang)
    {
        Set(lang);
        BrowserApi.ReloadPage();
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
        var game = BreakoutGame.Instance;
        if (game == null || EnabledLanguages.Count <= 2)
        {
            SelectNextAndSet();
            return;
        }

        var currentLang = AppSettings.Get(AppSettings.Lang, AppSettings.LangDefault);

        _ = Game.Dialogs.GameDialog.PopAll(game);

        var pickerTiles = new List<SkiaControl>();

        foreach (var lang in EnabledLanguages)
        {
            var isSelected = string.Equals(lang, currentLang, StringComparison.OrdinalIgnoreCase);

            pickerTiles.Add(new SelectableGameButton()
            {
                UseCache = SkiaCacheType.Image,
                CornerRadius = 8,
                BackgroundColor = Colors.Black,
                StrokeColor = isSelected
                    ? BreakoutGame.UiElements.ColorPrimary
                    : BreakoutGame.UiElements.ColorIconSecondary,
                StrokeWidth = isSelected ? 2 : 1,
                BevelType = BevelType.Bevel,
                Bevel = new SkiaBevel()
                {
                    Depth = 2,
                    LightColor = Colors.White,
                    ShadowColor = Colors.DarkBlue,
                    Opacity = 0.2,
                },
                WidthRequest = 86,
                MinimumWidthRequest = 86,
                HeightRequest = 82,
                FillGradient = new SkiaGradient()
                {
                    StartXRatio = 0,
                    EndXRatio = 1,
                    StartYRatio = 0,
                    EndYRatio = 0.55f,
                    Colors = isSelected
                        ? new[] { BreakoutGame.UiElements.ColorPrimary, BreakoutGame.UiElements.ColorPrimaryDark }
                        : new[] { Color.Parse("#3b3b6d"), Color.Parse("#222244") }
                },
                Children =
                {
                    new SkiaLayout()
                    {
                        Type = LayoutType.Column,
                        Spacing = 6,
                        Padding = new Thickness(10, 8),
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        Children =
                        {
                            new SkiaShape()
                            {
                                WidthRequest = 56,
                                HeightRequest = 28,
                                StrokeColor = BreakoutGame.UiElements.ColorIconSecondary,
                                StrokeWidth = 1,
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center,
                                Children =
                                {
                                    new SkiaSvg()
                                    {
                                        SvgString = AppSvg.GetFlag(lang),
                                        HorizontalOptions = LayoutOptions.Fill,
                                        VerticalOptions = LayoutOptions.Fill,
                                        Aspect = TransformAspect.Fill,
                                    }
                                }
                            },
                            new SkiaRichLabel()
                            {
                                Text = lang.ToUpperInvariant(),
                                FontFamily = AppFonts.Default,
                                FontSize = 14,
                                TextColor = AmstradColors.White,
                                HorizontalTextAlignment = DrawTextAlignment.Center,
                                VerticalOptions = LayoutOptions.Center,
                                HorizontalOptions = LayoutOptions.Fill,
                                UseCache = SkiaCacheType.Operations,
                                CharacterSpacing = 0.8,
                            },
                        }
                    }
                }
            }.WithGestures((me, args, apply) =>
            {
                if (args.Type == TouchActionResult.Tapped &&
                    !string.Equals(lang, currentLang, StringComparison.OrdinalIgnoreCase))
                {
                    SetAndApply(lang);
                }

                return me;
            }));
        }

        var pickerWrap = new SkiaLayout()
        {
            Type = LayoutType.Wrap,
            Spacing = 10,
            HorizontalOptions = LayoutOptions.Center,
            Children = pickerTiles,
        };

        var pickerContent = new SkiaLayout()
        {
            Type = LayoutType.Column,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Children = new List<SkiaControl>()
            {
                pickerWrap,
            }
        };

        Game.Dialogs.GameDialog.Show(game, pickerContent, ResStrings.BtnClose.ToUpperInvariant(), null,
            onOk: () => { Tasks.StartDelayed(TimeSpan.FromMilliseconds(50), () => { game.ShowOptions(); }); });
    }
}