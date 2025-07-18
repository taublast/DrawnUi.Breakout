using Breakout.Game.Dialogs;
using DrawnUi.Controls;
using DrawnUi.Views;
using HotPreview;

namespace Breakout.Game
{
#if PREVIEWS

    public partial class MainPage : BasePageReloadable
    {
        #region DIALOGS

        [Preview]
        public static void Dialog_Options() => GameAction(x => x.ShowOptions());

        #endregion

        [Preview]
        public static void State_Welcome() => ApplyPreviewState(new PreviewAppState() { GameState = GameState.Ready });

        [Preview]
        public static void State_Playing() => ApplyPreviewState(new PreviewAppState() { GameState = GameState.Playing });

        [Preview]
        public static void State_Paused() => ApplyPreviewState(new PreviewAppState() { GameState = GameState.Paused });

        [Preview]
        public static void State_Level_Complete() =>
            ApplyPreviewState(new PreviewAppState() { GameState = GameState.LevelComplete });
        
        [Preview]
        public static void State_Game_Lost() => GameAction(x => x.GameLost());

        [Preview]
        public static void State_Game_Complete() => GameAction(x => x.GameComplete());

        [Preview]
        public static void DemoPlay() => ApplyPreviewState(new PreviewAppState() { GameState = GameState.DemoPlay });

        [Preview]
        public static void Level_1() => GameAction(x => x.StartNewLevel(1));

        [Preview]
        public static void Level_2() => GameAction(x=>x.StartNewLevel(2));

        [Preview]
        public static void Level_3() => GameAction(x => x.StartNewLevel(3));

        [Preview]
        public static void Level_4() => GameAction(x => x.StartNewLevel(4));

        [Preview]
        public static void Level_5() => GameAction(x => x.StartNewLevel(5));

        [Preview]
        public static void Level_6() => GameAction(x => x.StartNewLevel(6));

        [Preview]
        public static void Level_7() => GameAction(x => x.StartNewLevel(7));

        [Preview]
        public static void Level_8() => GameAction(x => x.StartNewLevel(8));

        [Preview]
        public static void Level_9() => GameAction(x => x.StartNewLevel(9));

        [Preview]
        public static void Level_10() => GameAction(x => x.StartNewLevel(10));

        [Preview]
        public static void Level_11() => GameAction(x => x.StartNewLevel(11));

        [Preview]
        public static void Level_12() => GameAction(x => x.StartNewLevel(12));
 

        #region SPRITES

        [Preview]
        public static void Sprite_Paddle() => Preview(new PaddleSprite(), "Paddle sprite has initial position X-Center, Y-End");

        [Preview]
        public static void Sprite_Ball() => Preview(new BallSprite(), "Ball sprite has initial position X-Center, Y-End");

        [Preview]
        public static void Sprite_Brick() => Preview( BrickSprite.Create(), "Brick sprite has initial position X-Start, Y-Start");

        #endregion

        #region LANGUAGE

        [Preview]
        public static void Language_EN() => MainPage.RestartWithLanguage("en");

        [Preview]
        public static void Language_DE() => MainPage.RestartWithLanguage("de");

        [Preview]
        public static void Language_ES() => MainPage.RestartWithLanguage("es");

        [Preview]
        public static void Language_FR() => MainPage.RestartWithLanguage("fr");

        [Preview]
        public static void Language_RU() => MainPage.RestartWithLanguage("ru");

        [Preview]
        public static void Language_IT() => MainPage.RestartWithLanguage("it");

        [Preview]
        public static void Language_JA() => MainPage.RestartWithLanguage("ja");

        [Preview]
        public static void Language_KO() => MainPage.RestartWithLanguage("ko");

        [Preview]
        public static void Language_ZH() => MainPage.RestartWithLanguage("zh");

        #endregion

        public static SkiaViewSwitcher? ViewsContainer;

        public static SkiaLayout CreatePreviewWrapper(SkiaControl control, string comments)
        {
            return new SkiaStack()
            {
                Spacing = 0,
                BackgroundColor = Colors.DarkGrey,
                VerticalOptions = LayoutOptions.Fill,
                Children =
                {
                    // navbar
                    new SkiaLayer()
                    {
                        HeightRequest = 44,
                        UseCache = SkiaCacheType.Operations,
                        BackgroundColor = Colors.Black,
                        Children =
                        {
                            new SkiaRichLabel($"← {ResStrings.BtnGoBack}")
                                {
                                    FontSize = 16,
                                    VerticalOptions = LayoutOptions.Center,
                                    Padding = new(16, 0),
                                    UseCache = SkiaCacheType.Operations
                                }
                                .OnTapped(me => { _ = ViewsContainer?.PopPage(); })
                        }
                    },

                    control,

                    //overlay
                    new SkiaRichLabel(comments)
                    {
                        Opacity = 0.5,
                        FontSize = 16,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Fill,
                        HorizontalTextAlignment = DrawTextAlignment.Center,
                        Margin = new (32),
                        Padding = new(12),
                        BackgroundColor = Color.Parse("#22000000"),
                        TextColor = Colors.White,
                        UseCache = SkiaCacheType.Operations
                    },

                }
            };
        }

        private static void Preview(SkiaControl control, string comments = null)
        {
            if (ViewsContainer != null)
            {
                ViewsContainer.PushView(CreatePreviewWrapper(control, comments), true, false);
            }
        }

        private static void GameAction(Action<BreakoutGame> action)
        {
            var breakoutGame = BreakoutGame.Instance ??
                               throw new InvalidOperationException("BreakoutGame isn't initialized");

            _ = GameDialog.PopAllAsync(breakoutGame);

            action?.Invoke(breakoutGame);
        }

        private static void ApplyPreviewState(PreviewAppState previewAppState)
        {
            var breakoutGame = BreakoutGame.Instance ??
                               throw new InvalidOperationException("BreakoutGame isn't initialized");

            breakoutGame.ApplyPreviewState(previewAppState);
        }
    }

#endif
}