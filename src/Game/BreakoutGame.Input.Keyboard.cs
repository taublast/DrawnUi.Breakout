using Breakout.Game.Input;

namespace Breakout.Game
{
    public partial class BreakoutGame : MauiGame
    {
        /// <summary>
        /// A keyboard key was pressed
        /// </summary>
        /// <param name="mauiKey"></param>
        public override void OnKeyDown(MauiKey mauiKey)
        {
            var gameKey = MapToGame(mauiKey);

            GameKeysQueue.Enqueue(gameKey);
        }

        /// <summary>
        /// A keyboard key was released
        /// </summary>
        /// <param name="mauiKey"></param>
        public override void OnKeyUp(MauiKey mauiKey)
        {
            var gameKey = MapToGame(mauiKey);

            if (State == GameState.Playing || State == GameState.DemoPlay)
            {
                if (gameKey == GameKey.Left)
                    IsMovingLeft = false;
                else if (gameKey == GameKey.Right)
                    IsMovingRight = false;
            }
        }
    }
}