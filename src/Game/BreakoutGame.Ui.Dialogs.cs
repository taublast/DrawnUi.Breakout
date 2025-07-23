using AppoMobi.Specials;
using Breakout.Game.Dialogs;
using Breakout.Game.Input;
using Breakout.Helpers;
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
            PlaySound(Sound.Sad);

            // Show game over dialog
            var gameOverContent = UiElements.DialogPrompt(string.Format(ResStrings.MessageGameOver, Score));

            GameDialog.Show(this, gameOverContent, ResStrings.BtnPlayAgain.ToUpperInvariant(),
                onOk: () =>
                {
                    State = GameState.Playing;
                    ResetGame();
                });
        }

        async void ShowLevelCompleteDialog()
        {
            PlaySound(Sound.Joy);

            // Show level complete dialog
            var levelCompleteContent =
                UiElements.DialogPrompt(string.Format(ResStrings.MessageLevelComplete, Level - 1, Score, Level));
            if (await GameDialog.ShowAsync(this, levelCompleteContent, ResStrings.BtnContinue.ToUpperInvariant()))
            {
                // Start the new level
                State = GameState.Playing;
                StartNewLevel();
            }
        }

        /// <summary>
        /// Shows the options dialog with game settings like music toggle
        /// </summary>
        public void ShowOptions()
        {
            PlaySound(Sound.Dialog);

            _ = GameDialog.PopAll(this);

            // Pause the game if currently playing
            var lastState = State;
            State = GameState.Paused;
            IsMovingLeft = false;
            IsMovingRight = false;

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





        /// <summary>
        /// Creates the content for the options dialog
        ///
        ///
        ///
        /// 
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
                    new OptionWithTappable("LangFlag")
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
                                    Tag="LangFlag",
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
                                    AppLanguage.SelectAndSet();
                                }),
                        }
                    },

                    // SOUND setting row
                    new OptionWithTappable("SoundSwitch")
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
                                    Tag="SoundSwitch",
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

                    // Music setting row - selectable
                    new OptionWithTappable("MusicSwitch")
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
                                    Tag = "MusicSwitch",
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
                                                PlayMusicLooped(Level);
                                            }
                                            else
                                            {
                                                PlayMusicLooped(0);
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

                    // Press input mode for HUD
                    new OptionWithTappable("HudSwitch")
                    {
                        Type = LayoutType.Row,
                        Spacing = 15,
                        HorizontalOptions = LayoutOptions.Fill,
                        Children = new List<SkiaControl>
                        {
                            new SkiaRichLabel()
                            {
                                Text = ResStrings.PressHud.ToUpperInvariant(),
                                FontFamily = AppFonts.Default,
                                FontSize = 18,
                                TextColor = AmstradColors.White,
                                VerticalOptions = LayoutOptions.Center,
                                HorizontalOptions = LayoutOptions.Start,
                                UseCache = SkiaCacheType.Operations,
                            },

                            new GameSwitch()
                                {
                                    Tag="HudSwitch",
                                    HorizontalOptions = LayoutOptions.End,
                                    VerticalOptions = LayoutOptions.Center,
                                }
                                .Initialize(me =>
                                {
                                    if (_audioService != null)
                                    {
                                        me.IsToggled = AppSettings.Get(AppSettings.InputPressEnabled,
                                            AppSettings.InputPressEnabledDefault);
                                    }
                                })
                                .OnToggled((me, state) =>
                                {
                                    SetInputPressMode(state);
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