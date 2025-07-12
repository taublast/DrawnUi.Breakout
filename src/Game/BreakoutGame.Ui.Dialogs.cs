using Breakout.Game.Dialogs;

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

        #endregion
 
    }
}