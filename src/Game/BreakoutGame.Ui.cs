using System.Diagnostics;
using AppoMobi.Maui.Gestures;
using Breakout.Game.Controls;
using Breakout.Game.Dialogs;
using Breakout.Game.Input;
using SkiaSharp;

namespace Breakout.Game
{
    public partial class BreakoutGame : MauiGame
    {
        #region UI

        public BallSprite Ball;
        public PaddleSprite Paddle;
        public SkiaLayout GameField;
        private SkiaLayout BricksContainer;

        /// <summary>
        /// Need this when we change language
        /// </summary>
        public void RedrawFromOptions()
        {
            OnPropertyChanged(nameof(Level));
            OnPropertyChanged(nameof(Score));
            OnPropertyChanged(nameof(State));
            State = PreviousState;
            ShowOptions();
        }

        void CreateUi()
        {
            HorizontalOptions = LayoutOptions.Fill;
            VerticalOptions = LayoutOptions.Fill;
            BackgroundColor = Colors.DarkSlateBlue.WithAlpha(0.975f);

            Children = new List<SkiaControl>()
            {
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
                            IsClippedToBounds = true, // clip powerups
                            VerticalOptions = LayoutOptions.Fill,
                            //HeightRequest = 500,
                            Children =
                            {
                                //all bricks inside one layer draw cached if unchanged
                                new SkiaLayout()
                                {
                                    UseCache = SkiaCacheType.ImageComposite, //critical for perf
                                    HorizontalOptions = LayoutOptions.Center,
                                    Margin = new(0,90,0,0),
                                }.Assign(out BricksContainer),

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
                                            FontFamily = AppFonts.GameAutoselect,
                                            FontSize = 17 * AppFonts.GameAdjustSize,
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
                                            me.FontFamily = AppFonts.GameAutoselect;
                                            me.FontSize = 17 * AppFonts.GameAdjustSize;

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
                                            FontFamily = AppFonts.GameAutoselect,
                                            FontSize = 17* AppFonts.GameAdjustSize,
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
                                            me.FontFamily = AppFonts.GameAutoselect;
                                            me.FontSize = 17 * AppFonts.GameAdjustSize;

                                            me.Text = $"{ResStrings.Lev.ToUpperInvariant()}: {Level}";
                                        }),
                                 

                                        //LIVES
                                        new SkiaLayout()
                                        {
                                            UseCache = SkiaCacheType.Image,
                                            HorizontalOptions = LayoutOptions.Start,
                                            Type = LayoutType.Row,
                                            Spacing = 3,
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
                        new StableCacheLayout()
                        {
                            HeightRequest = 80,
                            HorizontalOptions = LayoutOptions.Fill,
                            UseCache = SkiaCacheType.GPU,
                            BackgroundColor = Color.Parse("#66000000"),
                            Children =
                            {
                                new SkiaSvg()
                                {
                                    Opacity = 0.85,
                                    UseCache = SkiaCacheType.Image,
                                    SvgString = App.Current.Resources.Get<string>("SvgSettings"),
                                    WidthRequest = 56,
                                    LockRatio = 1,
                                    VerticalOptions = LayoutOptions.Center,
                                    Margin = new (12,0,0,0),
                                }
                                .OnTapped(me =>
                                {
                                    TogglePause();

                                    //var dlgOptions = GameDialog.GetTopDialog(this);
                                    //if (dlgOptions!=null && dlgOptions.Content.Tag == "Options")
                                    //{
                                    //    _ = dlgOptions.CloseAsync(true, true);
                                    //}
                                    //else
                                    //{
                                    //    this.ShowOptions();
                                    //}
                                }),


                                //ARROWS
                                new HudArrows(this)
                                {
                                    Margin = new (80,0,16,0),
                                    HorizontalOptions = LayoutOptions.Fill,
                                    VerticalOptions = LayoutOptions.Fill,
                                    UseCache = SkiaCacheType.Operations,
                                    ColumnSpacing = 20,
                                    Children=
                                    {
                                        new SkiaSvg()
                                        {
                                            Opacity = 0.75,
                                            TintColor = UiElements.ColorPrimary,
                                            UseCache = SkiaCacheType.Image,
                                            SvgString = App.Current.Resources.Get<string>("SvgLeft"),
                                            WidthRequest = 56,
                                            LockRatio = 1,
                                            HorizontalOptions = LayoutOptions.End,
                                            VerticalOptions = LayoutOptions.Center,
                                        },
                                        new SkiaSvg()
                                        {
                                            HorizontalOptions = LayoutOptions.Start,
                                            TintColor = UiElements.ColorPrimary,
                                            Opacity = 0.75,
                                            UseCache = SkiaCacheType.Image,
                                            SvgString = App.Current.Resources.Get<string>("SvgRight"),
                                            WidthRequest = 56,
                                            LockRatio = 1,
                                            VerticalOptions = LayoutOptions.Center,
                                        }.WithColumn(1),
                                        
                                        //we will add hotspots in code-behind

                                    }
                                }.WithColumnDefinitions("50*, 50*"),


                                /*
                                new SkiaSvg()
                                    {
                                        Opacity = 0.75,
                                        UseCache = SkiaCacheType.Image,
                                        SvgString = App.Current.Resources.Get<string>("SvgUser"),
                                        WidthRequest = 56,
                                        LockRatio = 1,
                                        HorizontalOptions = LayoutOptions.End,
                                        VerticalOptions = LayoutOptions.Center,
                                        Margin = new (0,0,12,0),
                                    }
                                    .OnTapped(me =>
                                    {
                                        //profile
                                    })
                                */
                            }
                        }
                    }
                },
            };
        }

        #endregion

        public static class UiElements
        {

            public static Color ColorPrimary = Colors.HotPink;
            public static Color ColorPrimaryDark = Colors.DeepPink;

            public static Color ColorIconSecondary = Colors.DarkGray;

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
                return new SelectableGameButton()
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
                            FontSize = 14 * AppFonts.GameAdjustSize,
                            FontFamily = AppFonts.GameAutoselect,
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
                            UiElements.ColorPrimary,
                            UiElements.ColorPrimaryDark,
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

    public static class AmstradColors
    {
        public static readonly Color Black = Color.Parse("#000000");
        public static readonly Color Blue = Color.Parse("#0000FF");
        public static readonly Color Red = Color.Parse("#FF0000");
        public static readonly Color Magenta = Color.Parse("#FF00FF");
        public static readonly Color Green = Color.Parse("#00FF00");
        public static readonly Color Cyan = Color.Parse("#00FFFF");
        public static readonly Color Yellow = Color.Parse("#FFFF00");
        public static readonly Color White = Color.Parse("#FFFFFF");
        public static readonly Color Grey = Color.Parse("#808080");
        public static readonly Color BrightBlue = Color.Parse("#0080FF");
        public static readonly Color BrightRed = Color.Parse("#FF8080");
        public static readonly Color BrightMagenta = Color.Parse("#FF80FF");
        public static readonly Color BrightGreen = Color.Parse("#80FF80");
        public static readonly Color BrightCyan = Color.Parse("#80FFFF");
        public static readonly Color BrightYellow = Color.Parse("#FFFF80");
        public static readonly Color BrightWhite = Color.Parse("#C0C0C0");
        public static readonly Color DarkBlue = Color.Parse("#000080");
        public static readonly Color DarkRed = Color.Parse("#800000");
        public static readonly Color DarkMagenta = Color.Parse("#800080");
        public static readonly Color DarkGreen = Color.Parse("#008000");
        public static readonly Color DarkCyan = Color.Parse("#008080");
        public static readonly Color DarkYellow = Color.Parse("#808000");
        public static readonly Color DarkGrey = Color.Parse("#404040");
        public static readonly Color MidBlue = Color.Parse("#4040FF");
        public static readonly Color MidRed = Color.Parse("#FF4040");
        public static readonly Color MidGreen = Color.Parse("#40FF40");
        public static readonly Color MidCyan = Color.Parse("#40FFFF");

        // Optional: A method to get all colors as an array
        public static Color[] GetAllColors()
        {
            return new[]
            {
            Black, Blue, Red, Magenta, Green, Cyan, Yellow, White, Grey,
            BrightBlue, BrightRed, BrightMagenta, BrightGreen, BrightCyan, BrightYellow, BrightWhite,
            DarkBlue, DarkRed, DarkMagenta, DarkGreen, DarkCyan, DarkYellow, DarkGrey,
            MidBlue, MidRed, MidGreen, MidCyan
        };
        }
    }
}