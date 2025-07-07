using BreakoutGame.Game;
using DrawnUi.Views;
using PreviewFramework;

namespace BreakoutGame
{
    public class MainPage : BasePageReloadable
    {
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
                        new Game.BreakoutGame(),

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

#if PREVIEWS
        [Preview]
        public static void Welcome() => ApplyPreviewState(new PreviewAppState() { GameState = GameState.Ready });

        [Preview]
        public static void Playing() => ApplyPreviewState(new PreviewAppState() { GameState = GameState.Playing });

        [Preview]
        public static void Paused() => ApplyPreviewState(new PreviewAppState() { GameState = GameState.Paused });

        [Preview]
        public static void Ended() => ApplyPreviewState(new PreviewAppState() { GameState = GameState.Ended });

        [Preview]
        public static void LevelComplete() => ApplyPreviewState(new PreviewAppState() { GameState = GameState.LevelComplete });

        [Preview]
        public static void DemoPlay() => ApplyPreviewState(new PreviewAppState() { GameState = GameState.DemoPlay });

        [Preview]
        public static void Level2() => ApplyPreviewState(PreviewAppState.BeginningOfLevel(2));

        [Preview]
        public static void Level3() => ApplyPreviewState(PreviewAppState.BeginningOfLevel(3));

        [Preview]
        public static void Level4() => ApplyPreviewState(PreviewAppState.BeginningOfLevel(4));

        [Preview]
        public static void Level5() => ApplyPreviewState(PreviewAppState.BeginningOfLevel(5));

        [Preview]
        public static void Level6() => ApplyPreviewState(PreviewAppState.BeginningOfLevel(6));

        [Preview]
        public static void Level7() => ApplyPreviewState(PreviewAppState.BeginningOfLevel(7));

        [Preview]
        public static void Level8() => ApplyPreviewState(PreviewAppState.BeginningOfLevel(8));

        private static void ApplyPreviewState(PreviewAppState previewAppState)
        {
            var breakoutGame = Game.BreakoutGame.Instance ??
                throw new InvalidOperationException("BreakoutGame isn't initialized");

            breakoutGame.ApplyPreviewState(previewAppState);
        }
#endif
    }
}