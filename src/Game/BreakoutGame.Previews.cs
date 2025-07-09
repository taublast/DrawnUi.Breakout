using Breakout.Game.Dialogs;


namespace Breakout.Game
{

#if PREVIEWS

    public partial class BreakoutGame : MauiGame
    {
        public void ApplyPreviewState(PreviewAppState previewAppState)
        {
            ResetGame();

            _ = GameDialog.PopAllAsync(this);

            var newState = previewAppState.GameState;
            if (newState == GameState.Ready)
            {
                StartDemoMode();
                ShowWelcomeDialog();
            }
            else if (newState == GameState.DemoPlay)
            {
                StartDemoMode();
            }
            else if (newState == GameState.Playing)
            {
                StartNewGamePlayer();

                Lives = previewAppState.Lives;
                Score = previewAppState.Score;
            }
            else if (newState == GameState.Paused)
            {
                StartNewGamePlayer();

                Lives = previewAppState.Lives;
                Score = previewAppState.Score;

                PauseGame();
            }
            else if (newState == GameState.LevelComplete)
            {
                StartNewGamePlayer();
                State = GameState.LevelComplete;
                Level = previewAppState.Level;

                Lives = previewAppState.Lives;
                Score = previewAppState.Score;

                LevelComplete();
            }
        }
    }

    public record PreviewAppState
    {
        public GameState GameState { get; init; } = GameState.Ready;
        public int Level { get; init; } = 1;
        public int Lives { get; init; } = 3;
        public int Score { get; init; } = 1000;

        public static PreviewAppState BeginningOfLevel(int level)
        {
            return new PreviewAppState()
                { Level = level - 1, GameState = GameState.LevelComplete, Score = 1000 * (level - 1) };
        }
    }

#endif
}