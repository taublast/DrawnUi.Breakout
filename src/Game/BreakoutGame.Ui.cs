using System.Diagnostics;
using AppoMobi.Maui.Gestures;
using AppoMobi.Specials;
using SkiaSharp;
using System.Numerics;
using System.Runtime.CompilerServices;
using DrawnUi.Draw;
using BreakoutGame.Game.Dialogs;
using BreakoutGame.Game.Ai;

namespace BreakoutGame.Game
{
    public partial class BreakoutGame : MauiGame
    {
       
        #region DIALOGS

        void InitDialogs()
        {
            GameDialog.DefaultTemplate = DialogThemes.Game;
        }

        void ShowWelcomeDialog()
        {
            GameDialog.Show(this,
                UiElements.DialogPrompt(
                    "Welcome to Breakout!\nUse mouse or touch to move the paddle. Break all the bricks to win!"),
                "START GAME", onOk: () => { StartNewGamePlayer(); });
        }

        void ShowGameOverDialog()
        {
            // Show game over dialog
            var gameOverContent = UiElements.DialogPrompt($"Game Over!\nFinal Score: {Score}\nBetter luck next time!");

            GameDialog.Show(this, gameOverContent, "PLAY AGAIN", "QUIT",
                onOk: () => ResetGame(),
                onCancel: () =>
                {
                    // Could navigate back or close the game
                });
        }

        async void ShowLevelCompleteDialog()
        {
            // Show level complete dialog
            var levelCompleteContent =
                UiElements.DialogPrompt($"Level {Level - 1} Complete!\nScore: {Score}\nGet ready for Level {Level}!");
            if (await GameDialog.ShowAsync(this, levelCompleteContent, "CONTINUE"))
            {
                // Start the new level
                StartNewLevel();
                State = GameState.Playing;
                StartLoop();
            }
        }

        // Example of using the async dialog method
        async void ShowExampleAsyncDialog()
        {
            var content = UiElements.DialogPrompt("Do you want to continue?");

            bool result = await GameDialog.ShowAsync(this, content, "YES", "NO");

            if (result)
            {
                // User clicked YES
                // Do something...
            }
            else
            {
                // User clicked NO
                // Do something else...
            }
        }

        // Example of using the navigation stack
        void ShowStackedDialogs()
        {
            // Push first dialog
            var content1 = UiElements.DialogPrompt("This is the first dialog");

            GameDialog.Push(this, content1, "NEXT", "CANCEL",
                onOk: () =>
                {
                    // Push second dialog
                    var content2 = new SkiaLabel()
                    {
                        Text = "This is the second dialog",
                        TextColor = Colors.White,
                        FontSize = 16,
                        HorizontalTextAlignment = DrawTextAlignment.Center,
                        HorizontalOptions = LayoutOptions.Fill,
                    };

                    GameDialog.Push(this, content2, "FINISH", "BACK",
                        onOk: () =>
                        {
                            // Pop all dialogs
                            _ = GameDialog.PopAll(this);
                        },
                        onCancel: () =>
                        {
                            // Pop just this dialog (go back to first)
                            _ = GameDialog.Pop(this);
                        });
                },
                onCancel: () =>
                {
                    // Cancel everything
                    _ = GameDialog.PopAll(this);
                });
        }

        // Example of using different dialog themes
        void ShowThemeExamples()
        {
            var content = new SkiaLabel()
            {
                Text = "Choose a dialog theme to preview:",
                TextColor = Colors.White,
                FontSize = 16,
                HorizontalTextAlignment = DrawTextAlignment.Center,
                HorizontalOptions = LayoutOptions.Fill,
            };

            // Show theme selection dialog using default Game theme
            GameDialog.Show(this, content, "MODERN", "RETRO",
                onOk: () =>
                {
                    // Show Modern theme example
                    var modernContent = new SkiaLabel()
                    {
                        Text = "This is the Modern theme!\nClean, contemporary styling with smooth animations.",
                        TextColor = Colors.Black,
                        FontSize = 16,
                        HorizontalTextAlignment = DrawTextAlignment.Center,
                        HorizontalOptions = LayoutOptions.Fill,
                    };
                    GameDialog.Show(this, modernContent, "NICE!", template: DialogThemes.Modern);
                },
                onCancel: () =>
                {
                    // Show Retro theme example
                    var retroContent = new SkiaLabel()
                    {
                        Text = "This is the Retro theme!\nTerminal-style green text on black background.",
                        TextColor = Colors.LimeGreen,
                        FontSize = 14,
                        FontFamily = "FontGame",
                        HorizontalTextAlignment = DrawTextAlignment.Center,
                        HorizontalOptions = LayoutOptions.Fill,
                    };
                    GameDialog.Show(this, retroContent, "COOL!", template: DialogThemes.Retro);
                });
        }

        #endregion

        #region UI

        /// <summary>
        /// Score can change several times per frame
        /// so we dont want bindings to update the score toooften.
        /// Instead we update the display manually once after the frame is finalized.
        /// </summary>
        void UpdateScore()
        {
            if (State == GameState.DemoPlay)
            {
                LabelScore.Text = $"DEMO PLAY";
            }
            else
            {
                //var collisionSystem = USE_RAYCAST_COLLISION ? "RAYCAST" : "AABB";
                LabelScore.Text = $"{ScoreLocalized}"; // | {collisionSystem}";
                //LabelHiScore.Text = HiScoreLocalized; //todo?
            }
        }

        void CreateUi()
        {
            HorizontalOptions = LayoutOptions.Fill;
            VerticalOptions = LayoutOptions.Fill;
            BackgroundColor = Colors.DarkBlue;

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
                            BackgroundColor = Colors.DarkSlateBlue,
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

                                //SCORE top bar
                                new SkiaLayer()
                                {
                                    ZIndex = 110,
                                    UseCache = SkiaCacheType.GPU,
                                    Children =
                                    {
                                        new SkiaLabel()
                                        {
                                            Margin = 16,
                                            FontFamily = "FontGame",
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
                                        }.Assign(out LabelScore)
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
                return new SkiaMarkdownLabel()
                {
                    Text = prompt,
                    UseCache = SkiaCacheType.Image,
                    TextColor = Colors.White,
                    //FontFamily = "FontText",
                    LineHeight = 1.25,
                    CharacterSpacing = 1.33,
                    FontSize = 26,
                    //StrokeWidth = 2,
                    //StrokeColor = Color.Parse("#999999"),
                    //DropShadowSize = 0,
                    //DropShadowColor = Color.Parse("#666666"),
                    HorizontalTextAlignment = DrawTextAlignment.Center,
                    HorizontalOptions = LayoutOptions.Fill,
                    //BackgroundColor = Colors.Pink,
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
                        new SkiaMarkdownLabel()
                        {
                            Text = caption,
                            Margin = new Thickness(16, 10),
                            UseCache = SkiaCacheType.Operations,
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions = LayoutOptions.Center,
                            FontSize = 14,
                            FontFamily = "FontGame",
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