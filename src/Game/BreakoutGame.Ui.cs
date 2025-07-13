﻿using System.Diagnostics;
using AppoMobi.Maui.Gestures;

namespace Breakout.Game
{
    public partial class BreakoutGame : MauiGame
    {
        #region UI

        public BallSprite Ball;
        public PaddleSprite Paddle;
        protected SkiaLayout GameField;

        void CreateUi()
        {
            HorizontalOptions = LayoutOptions.Fill;
            VerticalOptions = LayoutOptions.Fill;
            BackgroundColor = Colors.DarkSlateBlue.WithAlpha(0.975f);

            Children = new List<SkiaControl>()
            {
                //background image for different window size
                //can place background image..
                new SkiaLayer()
                {
                    UseCache = SkiaCacheType.Operations,
                    VerticalOptions = LayoutOptions.Fill,
                },

                //game and controls below
                new SkiaStack()
                {
                    Tag = "GameStack",
                    VerticalOptions = LayoutOptions.Fill,
                    Spacing = 0,
                    Children =
                    {
                        //GAME FIELD
                        new SkiaLayer()
                        {
                            VerticalOptions = LayoutOptions.Fill,
                            //HeightRequest = 500,
                            Children =
                            {
                                new BallSprite()
                                {
                                    ZIndex = 4
                                }.Assign(out Ball),

                                new PaddleSprite()
                                {
                                    ZIndex = 5,
                                    Top = -28
                                }.Assign(out Paddle),

                                //SCORE TOP BAR
                                new SkiaLayer()
                                {
                                    ZIndex = 110,
                                    UseCache = SkiaCacheType.GPU,
                                    Children =
                                    {
                                        //SCORE/DEMO
                                        new SkiaRichLabel()
                                        {
                                            UseCache = SkiaCacheType.Operations,
                                            Margin = 16,
                                            FontFamily = AppFonts.Game,
                                            FontSize = 17,
                                            StrokeColor = AmstradColors.DarkBlue,
                                            TextColor = AmstradColors.White,
                                            DropShadowColor = Colors.DarkBlue,
                                            DropShadowOffsetX = 2,
                                            DropShadowOffsetY = 2,
                                            DropShadowSize = 2,
                                            FillGradient = new()
                                            {
                                                Colors = new List<Color>()
                                                {
                                                    Colors.White,
                                                    Colors.CornflowerBlue
                                                }
                                            }
                                        }
                                        .ObserveProperties(this, [nameof(Score), nameof(State)], me =>
                                        {
                                            if (State == GameState.DemoPlay)
                                            {
                                                me.Text = ResStrings.DemoMode.ToUpperInvariant();
                                            }
                                            else
                                            {
                                                me.Text = $"{ResStrings.Score.ToUpperInvariant()}: {Score:0}";
                                            }
                                        }),

                                        //LEVEL
                                        new SkiaRichLabel()
                                        {
                                            UseCache = SkiaCacheType.Operations,
                                            Margin = 16,
                                            HorizontalOptions = LayoutOptions.End,
                                            FontFamily = AppFonts.Game,
                                            FontSize = 17,
                                            StrokeColor = AmstradColors.DarkBlue,
                                            TextColor = AmstradColors.White,
                                            DropShadowColor = Colors.DarkBlue,
                                            DropShadowOffsetX = 2,
                                            DropShadowOffsetY = 2,
                                            DropShadowSize = 2,
                                            FillGradient = new()
                                            {
                                                Colors = new List<Color>()
                                                {
                                                    Colors.White,
                                                    Colors.CornflowerBlue
                                                }
                                            }
                                        }
                                        .ObserveProperty(this, nameof(Level), me =>
                                        {
                                            me.Text = $"{ResStrings.Lev.ToUpperInvariant()}: {Level}";
                                        }),

                                        //LIVES
                                        new SkiaLayout()
                                        {
                                            UseCache = SkiaCacheType.Image,
                                            HorizontalOptions = LayoutOptions.Start,
                                            Type = LayoutType.Row,
                                            Spacing = 4,
                                            Margin = new (16,60,16,0),
                                            ItemTemplateType = typeof(LifeSprite)
                                        }
                                        .ObserveProperties(this, [nameof(Lives), nameof(State)], me =>
                                        {
                                            //if (State == GameState.DemoPlay)
                                            //{
                                            //    me.IsVisible = false;
                                            //}
                                            //else
                                            {
                                                me.IsVisible = true;
                                                me.ItemsSource = Enumerable.Repeat(1, Lives).ToArray();
                                            }
                                            Debug.WriteLine($"LIVES: {Lives}");
                                        }),
                                    }
                                }
                            }
                        }.Assign(out GameField),

                        //CONTROLS
                        new SkiaLayer()
                        {
                            HeightRequest = 80,
                            BackgroundColor = Color.Parse("#66000000")
                        }
                    }
                },
            };
        }

        #endregion

        public static class UiElements
        {
            public static SkiaControl DialogPrompt(string prompt)
            {
                return new SkiaRichLabel() //will auto-find installed font for missing glyphs
                {
                    Text = prompt,
                    UseCache = SkiaCacheType.Image,
                    HorizontalTextAlignment = DrawTextAlignment.Center,
                    HorizontalOptions = LayoutOptions.Fill,
                    FontFamily = AppFonts.Default,
                    LineHeight = 1,
                    LineSpacing = 0,
                    CharacterSpacing = 1.33,
                    FontSize = 18,
                    TextColor = AmstradColors.White,

                    //StrokeColor = AmstradColors.DarkBlue,
                    //DropShadowColor = Colors.DarkBlue,
                    //DropShadowOffsetX = 1,
                    //DropShadowOffsetY = 1,
                    //DropShadowSize = 1,
                };
            }

            public static void SetButtonPressed(SkiaShape btn)
            {
                btn.Children[0].TranslationX = 1;
                btn.Children[0].TranslationY = 1;
                btn.BevelType = BevelType.Emboss;
            }

            public static void SetButtonReleased(SkiaShape btn)
            {
                btn.Children[0].TranslationX = 0;
                btn.Children[0].TranslationY = 0;
                btn.BevelType = BevelType.Bevel;
            }

            public static SkiaShape Button(string caption, Action action)
            {
                return new SkiaShape()
                {
                    UseCache = SkiaCacheType.Image,
                    CornerRadius = 8,
                    MinimumWidthRequest = 100,
                    BackgroundColor = Colors.Black,
                    BevelType = BevelType.Bevel,
                    Bevel = new SkiaBevel()
                    {
                        Depth = 2,
                        LightColor = Colors.White,
                        ShadowColor = Colors.DarkBlue,
                        Opacity = 0.33,
                    },
                    Children =
                    {
                        new SkiaRichLabel()
                        {
                            Text = caption,
                            Margin = new Thickness(16, 8,16,10),
                            UseCache = SkiaCacheType.Operations,
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions = LayoutOptions.Center,
                            FontSize = 14,
                            FontFamily = AppFonts.Game,
                            TextColor = Colors.White,
                        }
                    },
                    FillGradient = new SkiaGradient()
                    {
                        StartXRatio = 0,
                        EndXRatio = 1,
                        StartYRatio = 0,
                        EndYRatio = 0.5f,
                        Colors = new Color[]
                        {
                            Colors.HotPink,
                            Colors.DeepPink,
                        }
                    },
                }.WithGestures((me, args, b) =>
                {
                    if (args.Type == TouchActionResult.Tapped)
                    {
                        action?.Invoke();
                    }
                    else if (args.Type == TouchActionResult.Down)
                    {
                        SetButtonPressed(me);
                    }
                    else if (args.Type == TouchActionResult.Up)
                    {
                        SetButtonReleased(me);
                        return null;
                    }

                    return me;
                });
            }
        }
    }
}