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

        /// <summary>
        /// Shows the options dialog with game settings like music toggle
        /// </summary>
        public void ShowOptions()
        {
            _ = GameDialog.PopAll(this);

            // Pause the game if currently playing
            var lastState = State;
            State = GameState.Paused;
            _moveLeft = false;
            _moveRight = false;

            // Create options dialog content
            var optionsContent = CreateOptionsDialogContent();

            void OnClose()
            {
                Tasks.StartDelayed(TimeSpan.FromMilliseconds(50), () =>
                {
                    TogglePause();
                });
            }

            // Show the dialog
            GameDialog.Show(this, optionsContent, ResStrings.BtnClose.ToUpperInvariant(), null,
                onOk: () =>
                {
                    OnClose();
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
                Tag = "Options",
                Type = LayoutType.Column,
                Spacing = 20,
                Padding = new Thickness(16,16,16,-12),
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
                                .OnTapped(me => { MainPage.SelectAndSetCountry(); }),
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
                                        AppSettings.Set(AppSettings.MusicOn, state);
                                        if (state)
                                        {
                                            if (PreviousState == GameState.Playing)
                                            {
                                                StartBackgroundMusic(Level);
                                            }
                                            else
                                            {
                                                StartBackgroundMusic(0);
                                            }
                                        }
                                        else
                                        {
                                            StopBackgroundMusic();
                                        }
                                    }
                                }),
                        }
                    },

                    //DEMO
                    UiElements.Button(ResStrings.DemoMode.ToUpperInvariant(), async () =>
                    {
                        StartNewGameDemo();
                    }).FillX().WithMargin(new Thickness(0,16,0,-16)),

                    //RESTART
                    UiElements.Button(ResStrings.NewGame.ToUpperInvariant(), async () =>
                    {
                        StartNewGamePlayer();
                    }).FillX().WithMargin(new Thickness(0,16,0,0)),

                }
            };

            return optionsLayout;
        }

        #endregion
    }
}