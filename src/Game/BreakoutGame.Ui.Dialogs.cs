using AppoMobi.Specials;
using Breakout.Game.Dialogs;
using DrawnUi.Controls;
using System.Globalization;

namespace Breakout.Game
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
#if WINDOWS || MACCATALYST
            var message = ResStrings.MessageWelcomeDesktop;
#else
            var message = ResStrings.MessageWelcome;
#endif
            GameDialog.Show(this,
                UiElements.DialogPrompt(message),
                ResStrings.StartGame.ToUpperInvariant(), onOk: () => { StartNewGamePlayer(); });
        }

        void ShowGameOverDialog()
        {
            // Show game over dialog
            var gameOverContent = UiElements.DialogPrompt(string.Format(ResStrings.MessageGameOver, Score));

            GameDialog.Show(this, gameOverContent, ResStrings.BtnPlayAgain.ToUpperInvariant(),
                onOk: () => ResetGame());
        }

        async void ShowLevelCompleteDialog()
        {
            // Show level complete dialog
            var levelCompleteContent =
                UiElements.DialogPrompt(string.Format(ResStrings.MessageLevelComplete, Level - 1, Score, Level));
            if (await GameDialog.ShowAsync(this, levelCompleteContent, ResStrings.BtnContinue.ToUpperInvariant()))
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
                    var content2 = new SkiaRichLabel()
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
            var content = new SkiaRichLabel()
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
                    var modernContent = new SkiaRichLabel()
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
                    var retroContent = new SkiaRichLabel()
                    {
                        Text = "This is the Retro theme!\nTerminal-style green text on black background.",
                        TextColor = Colors.LimeGreen,
                        FontSize = 14,
                        FontFamily = AppFonts.Game,
                        HorizontalTextAlignment = DrawTextAlignment.Center,
                        HorizontalOptions = LayoutOptions.Fill,
                    };
                    GameDialog.Show(this, retroContent, "COOL!", template: DialogThemes.Retro);
                });
        }

        /// <summary>
        /// Shows the options dialog with game settings like music toggle
        /// </summary>
        public void ShowOptions()
        {
            _ = GameDialog.PopAll(this);

            // Pause the game if currently playing
            var lastState = State;
            var wasPlaying = State == GameState.Playing;
            if (wasPlaying)
            {
                State = GameState.Paused;
                _moveLeft = false;
                _moveRight = false;
            }

            // Create options dialog content
            var optionsContent = CreateOptionsDialogContent();

            // Show the dialog
            GameDialog.Show(this, optionsContent, ResStrings.BtnOk.ToUpperInvariant(), null,
                onOk: () =>
                {
                    // Resume game if it was playing before
                    if (wasPlaying)
                    {
                        State = GameState.Playing;
                    }
                    else
                    {
                        State = lastState;
                        if (State == GameState.LevelComplete)
                        {
                            ShowLevelCompleteDialog();
                        }
                        else
                        {
                            ShowWelcomeDialog();
                        }
                    }
                });
        }

        public class DisplayFlag : SkiaLayout
        {
            private string _lang;

            public string Lang
            {
                get => _lang;
                set
                {
                    if (value == _lang) return;
                    _lang = value;
                    OnPropertyChanged();
                }
            }

            public DisplayFlag()
            {
                HeightRequest = 28;
                WidthRequest = 56;
                Children = new List<SkiaControl>()
                {
                    new SkiaLayout()
                    {
                        Type = LayoutType.Row,
                        VerticalOptions = LayoutOptions.Fill,
                        HorizontalOptions = LayoutOptions.Fill,
                        Spacing = 0,

                        Children = new List<SkiaControl>()
                        {
                            //flag icon
                            new SkiaShape()
                            {
                                Margin = new(0, 0, 2, 0),
                                StrokeColor = UiElements.ColorIconSecondary,
                                StrokeWidth = 1,
                                VerticalOptions = LayoutOptions.Fill,
                                HorizontalOptions = LayoutOptions.Fill,
                                Children =
                                {
                                    new SkiaSvg()
                                        {
                                            VerticalOptions = LayoutOptions.Fill,
                                            HorizontalOptions = LayoutOptions.Fill,
                                            Aspect = TransformAspect.Fill
                                        }
                                        .ObserveProperty(this, nameof(Lang), me =>
                                        {
                                            if (!string.IsNullOrEmpty(this.Lang))
                                            {
                                                var resKey = $"SvgFlag{this.Lang.ToTitleCase()}";
                                                me.SvgString = App.Current.Resources.Get<string>(resKey);
                                            }
                                        }),
                                }
                            },


                            //dropdown icon
                            new SkiaSvg()
                            {
                                Margin = new Microsoft.Maui.Thickness(1, 1, 0, 0),
                                HorizontalOptions = LayoutOptions.Start,
                                TintColor = UiElements.ColorIconSecondary,
                                VerticalOptions = LayoutOptions.Fill,
                                WidthRequest = 10,
                                SvgString = App.Current.Resources.Get<string>("SvgDropdown")
                            }
                        },
                    }
                };
            }
        }

        public class GameSwitch : SkiaSwitch
        {
            public GameSwitch()
            {
                WidthRequest = 60;
                HeightRequest = 32;
                ColorFrameOff = UiElements.ColorIconSecondary;
                ColorFrameOn = UiElements.ColorPrimary;
                ColorThumbOff = AmstradColors.White;
                ColorThumbOn = AmstradColors.White;
                UseCache = SkiaCacheType.Operations;
            }
        }

        /// <summary>
        /// Creates the content for the options dialog
        /// </summary>
        private SkiaControl CreateOptionsDialogContent()
        {
            // Create the options layout
            var optionsLayout = new SkiaLayout()
            {
                Type = LayoutType.Column,
                Spacing = 20,
                Padding = new Thickness(20),
                HorizontalOptions = LayoutOptions.Fill,
                Children = new List<SkiaControl>
                {
                    // Title
                    new SkiaRichLabel()
                    {
                        Text = ResStrings.Options.ToUpperInvariant(),
                        FontFamily = AppFonts.Default,
                        FontSize = 24,
                        TextColor = AmstradColors.White,
                        HorizontalTextAlignment = DrawTextAlignment.Center,
                        HorizontalOptions = LayoutOptions.Fill,
                        UseCache = SkiaCacheType.Operations
                    },

                    // LANGUAGE setting row
                    new SkiaLayout()
                    {
                        Type = LayoutType.Row,
                        Spacing = 15,
                        HorizontalOptions = LayoutOptions.Fill,
                        Children = new List<SkiaControl>
                        {
                            //SOUND FX
                            new SkiaRichLabel()
                            {
                                Text = ResStrings.Language.ToUpperInvariant(),
                                FontFamily = AppFonts.Default,
                                FontSize = 18,
                                TextColor = AmstradColors.White,
                                VerticalOptions = LayoutOptions.Center,
                                HorizontalOptions = LayoutOptions.Start,
                                UseCache = SkiaCacheType.Operations,
                            },
                            new DisplayFlag()
                                {
                                    HorizontalOptions = LayoutOptions.End,
                                    VerticalOptions = LayoutOptions.Center,
                                }
                                .Initialize(me =>
                                {
                                    var lang = AppSettings.Get(AppSettings.Lang, AppSettings.LangDefault);
                                    me.Lang = lang;
                                })
                                .OnTapped(me =>
                                {
                                    MainPage.SelectAndSetCountry();
                                }),
                        }
                    },

                    // SOUND setting row
                    new SkiaLayout()
                    {
                        Type = LayoutType.Row,
                        Spacing = 15,
                        HorizontalOptions = LayoutOptions.Fill,
                        Children = new List<SkiaControl>
                        {
                            //SOUND FX
                            new SkiaRichLabel()
                            {
                                Text = ResStrings.Sounds.ToUpperInvariant(),
                                FontFamily = AppFonts.Default,
                                FontSize = 18,
                                TextColor = AmstradColors.White,
                                VerticalOptions = LayoutOptions.Center,
                                HorizontalOptions = LayoutOptions.Start,
                                UseCache = SkiaCacheType.Operations,
                            },
                            new GameSwitch()
                                {
                                    HorizontalOptions = LayoutOptions.End,
                                    VerticalOptions = LayoutOptions.Center,
                                }
                                .Initialize(me =>
                                {
                                    if (_audioService != null)
                                    {
                                        me.IsToggled = AppSettings.Get(AppSettings.SoundsOn,
                                            AppSettings.SoundsOnDefault);
                                    }
                                })
                                .OnToggled((me, state) =>
                                {
                                    if (_audioService != null)
                                    {
                                        EnableSounds(state);
                                        AppSettings.Set(AppSettings.SoundsOn, state);
                                    }
                                }),
                        }
                    },

                    // Music setting row
                    new SkiaLayout()
                    {
                        Type = LayoutType.Row,
                        Spacing = 15,
                        HorizontalOptions = LayoutOptions.Fill,
                        Children = new List<SkiaControl>
                        {
                            //MUSIC
                            new SkiaRichLabel()
                            {
                                Text = ResStrings.Music.ToUpperInvariant(),
                                FontFamily = AppFonts.Default,
                                FontSize = 18,
                                TextColor = AmstradColors.White,
                                VerticalOptions = LayoutOptions.Center,
                                HorizontalOptions = LayoutOptions.Start,
                                UseCache = SkiaCacheType.Operations,
                            },
                            new GameSwitch()
                                {
                                    HorizontalOptions = LayoutOptions.End,
                                    VerticalOptions = LayoutOptions.Center,
                                }
                                .Initialize(me =>
                                {
                                    if (_audioService != null)
                                    {
                                        me.IsToggled = AppSettings.Get(AppSettings.MusicOn,
                                            AppSettings.MusicOnDefault);
                                    }
                                })
                                .OnToggled((me, state) =>
                                {
                                    if (_audioService != null)
                                    {
                                        SetupBackgroundMusic(state);
                                        AppSettings.Set(AppSettings.MusicOn, state);
                                    }
                                }),
                        }
                    }
                }
            };

            return optionsLayout;
        }


        #endregion
    }
}