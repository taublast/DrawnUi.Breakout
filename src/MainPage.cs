﻿using Breakout.Game.Dialogs;
using DrawnUi.Controls;
using DrawnUi.Views;
using HotPreview;
using System.Globalization;
using Breakout;

namespace Breakout.Game
{
    public partial class MainPage : BasePageReloadable
    {
        public MainPage()
        {
            Instance = this;

            AppLanguage.ApplySelected();
        }

        /// <summary>
        /// To change language of this one
        /// </summary>
        public static MainPage Instance;

        Canvas Canvas;

        public override void Build()
        {
            Canvas?.Dispose();

            Canvas = new RescalingCanvas()
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
                        //WALLPAPER
                        new SkiaImage(@"Images/back.jpg")
                        {
                            UseCache = SkiaCacheType.Image,
                            AddEffect = SkiaImageEffect.Darken,
                            Darken = 0.2
                        }.Fill(),

                        //MAIN VIEW
                        new SkiaViewSwitcher()
                        {
                            HorizontalOptions = LayoutOptions.Center,
                            WidthRequest = 360,
                            HeightRequest = 760,
                            VerticalOptions = LayoutOptions.Center,
                            SelectedIndex = 0,
                            Children =
                            {
                                new Game.BreakoutGame(),
                            }
                        }.Assign(out ViewsContainer),
#if DEBUG
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
#endif
                    }
                }.Fill()
            };

            this.Content = Canvas;
        }

        [AutoGeneratePreview(false)]
        public class RescalingCanvas : Canvas
        {
            public float GameScale { get; set; } = 1;

            protected override void Draw(DrawingContext context)
            {
                var wantedHeight = Breakout.Game.BreakoutGame.HEIGHT * context.Scale;
                var wantedWidth = Breakout.Game.BreakoutGame.WIDTH * context.Scale;

                var scaleWidth = this.DrawingRect.Width / wantedWidth;
                var scaleHeight = this.DrawingRect.Height / wantedHeight;

                GameScale = Math.Min(scaleWidth, scaleHeight);

                context.Scale *= GameScale;

                base.Draw(context);
            }
        }

    }



}