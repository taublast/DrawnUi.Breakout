using AppoMobi.Specials;
using Breakout.Game.Dialogs;
using Breakout.Game.Input;
using Breakout.Helpers;
#if !BROWSER
using DrawnUi.Controls;
#endif
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
#if WINDOWS || MACCATALYST || BROWSER
            var message = ResStrings.MessageWelcomeDesktop;
#else
            var message = ResStrings.MessageWelcome;
#endif
            GameDialog.Show(this,
                UiElements.DialogPrompt(message),
                ResStrings.StartGame.ToUpperInvariant(), onOk: () =>
                {
                    NotifyAudioUserGesture();
                    StartNewGamePlayer();
                });
        }

        void ShowStartupAssetFailureDialog(StartupAssetFailure failure, Action? onOk = null)
        {
            var content = new SkiaLayout()
            {
                Type = LayoutType.Column,
                Spacing = 12,
                HorizontalOptions = LayoutOptions.Fill,
                Children =
                {
                    new SkiaRichLabel()
                    {
                        Text = failure.Title.ToUpperInvariant(),
                        FontFamily = AppFonts.Default,
                        FontSize = 20,
                        TextColor = AmstradColors.White,
                        HorizontalTextAlignment = DrawTextAlignment.Center,
                        HorizontalOptions = LayoutOptions.Fill,
                        UseCache = SkiaCacheType.Operations,
                    },

                    UiElements.DialogPrompt(failure.Message),

                    new SkiaRichLabel()
                    {
                        Text = failure.Details ?? string.Empty,
                        FontFamily = AppFonts.Default,
                        FontSize = 13,
                        TextColor = Colors.White.WithAlpha(0.8f),
                        HorizontalTextAlignment = DrawTextAlignment.Center,
                        HorizontalOptions = LayoutOptions.Fill,
                        UseCache = SkiaCacheType.Operations,
                        IsVisible = !string.IsNullOrWhiteSpace(failure.Details),
                    }
                }
            };

            GameDialog.Show(this, content, ResStrings.BtnOk.ToUpperInvariant(), onOk: () =>
            {
                onOk?.Invoke();
            });
        }

        void ShowGameOverDialog()
        {
            PlaySound(Sound.Sad);

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

            var levelCompleteContent =
                UiElements.DialogPrompt(string.Format(ResStrings.MessageLevelComplete, Level - 1, Score, Level));
            if (await GameDialog.ShowAsync(this, levelCompleteContent, ResStrings.BtnContinue.ToUpperInvariant()))
            {
                State = GameState.Playing;
                StartNewLevel();
            }
        }

        public void ShowOptions()
        {
            PlaySound(Sound.Dialog);

            _ = GameDialog.PopAll(this);

            State = GameState.Paused;
            IsMovingLeft = false;
            IsMovingRight = false;

            var optionsContent = CreateOptionsDialogContent();

            void OnClose()
            {
                Tasks.StartDelayed(TimeSpan.FromMilliseconds(50), () =>
                {
                    TogglePause();
                });
            }

            GameDialog.Show(this, optionsContent, ResStrings.BtnClose.ToUpperInvariant(), null,
                onOk: () =>
                {
                    OnClose();
                });
        }

        private SkiaControl CreateOptionsDialogContent()
        {
            var optionsLayout = new SkiaLayout()
            {
                Tag = "Options",
                Type = LayoutType.Column,
                Spacing = 20,
                Padding = new Thickness(12,0,12,-12),
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
                        HorizontalOptions = LayoutOptions.Fill,
                        Children = new List<SkiaControl>
                        {
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
                                .OnTapped((me, a) =>
                                {
                                    if (a.Parameters.Event != null)
                                    {
                                        AppLanguage.SelectAndSet();
                                    }
                                    else
                                    {
                                        AppLanguage.SelectNextAndSet();
                                    }
                                }),
                        }
                    },

                    // SOUND setting row
                    new OptionWithTappable("SoundSwitch")
                    {
                        HorizontalOptions = LayoutOptions.Fill,
                        Children = new List<SkiaControl>
                        {
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
                                    me.Toggled += (_, state) =>
                                    {
                                        if (_audioService != null)
                                        {
                                            EnableSounds(state);
                                            AppSettings.Set(AppSettings.SoundsOn, state);
                                        }
                                    };
                                }),
                        }
                    },

                    // Music setting row
                    new OptionWithTappable("MusicSwitch")
                    {
                        HorizontalOptions = LayoutOptions.Fill,
                        Children = new List<SkiaControl>
                        {
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
                                    me.Toggled += (_, state) =>
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
                                    };
                                }),
                        }
                    },

                    // Press input mode for HUD
                    new OptionWithTappable("HudSwitch")
                    {
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
                                    me.IsToggled = AppSettings.Get(AppSettings.InputPressEnabled,
                                        AppSettings.InputPressEnabledDefault);

                                    me.Toggled += (_, state) => { SetInputPressMode(state); };
                                }),
                        }
                    },

#if BROWSER
                    new OptionWithTappable("BrowserFullscreenSwitch")
                    {
                        HorizontalOptions = LayoutOptions.Fill,
                        Children = new List<SkiaControl>
                        {
                            new SkiaRichLabel()
                            {
                                Text = ResStrings.Fullscreen.ToUpperInvariant(),
                                FontFamily = AppFonts.Default,
                                FontSize = 18,
                                TextColor = AmstradColors.White,
                                VerticalOptions = LayoutOptions.Center,
                                HorizontalOptions = LayoutOptions.Start,
                                UseCache = SkiaCacheType.Operations,
                            },

                            new GameSwitch()
                                {
                                    Tag = "BrowserFullscreenSwitch",
                                    HorizontalOptions = LayoutOptions.End,
                                    VerticalOptions = LayoutOptions.Center,
                                }
                                .Initialize(me =>
                                {
                                    me.IsToggled = BrowserFullscreen.IsEnabled();
                                    me.Toggled += (_, state) =>
                                    {
                                        BrowserFullscreen.SetEnabled(state);
                                    };
                                }),
                        }
                    },
#endif

                    new SkiaRichLabel()
                    {
                        Text = AppVersion.Current,
                        FontFamily = AppFonts.Default,
                        FontSize = 10,
                        TextColor = AmstradColors.White,
                        HorizontalOptions = LayoutOptions.Center,
                        UseCache = SkiaCacheType.Operations,
                        Margin = new Thickness(0, 0, 0, -8)
                    },

                    //RESTART
                    UiElements.Button(ResStrings.NewGame.ToUpperInvariant(), async () =>
                    {
                        NotifyAudioUserGesture();
                        StartNewGamePlayer();
                    }).FillX().WithMargin(new Thickness(0,16,0,-16)),

                    //DEMO
                    UiElements.Button(ResStrings.DemoMode.ToUpperInvariant(), async () =>
                    {
                        NotifyAudioUserGesture();
                        StartNewGameDemo();
                    }).FillX().WithMargin(new Thickness(0,16,0,0)),


                }
            };

            return optionsLayout;
        }

        #endregion
    }
}
