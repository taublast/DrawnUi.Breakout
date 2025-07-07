using BreakoutGame.Game;

#if PREVIEWS
namespace BreakoutGame
{
    public record PreviewAppState
    {
        public GameState GameState { get; init; } = GameState.Ready;
        public int Level { get; init; } = 1;
        public int Lives { get; init; } = 3;
        public int Score { get; init; } = 1000;

        public static PreviewAppState BeginningOfLevel(int level)
        {
            return new PreviewAppState() { Level = level - 1, GameState = GameState.LevelComplete, Score = 1000 * (level - 1) };
        }
    }
}
#endif
