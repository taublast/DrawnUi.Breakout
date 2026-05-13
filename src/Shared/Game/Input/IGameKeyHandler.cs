namespace Breakout.Game.Input
{
    /// <summary>
    /// Interface for controls that can handle GameKey input (like dialogs)
    /// </summary>
    public interface IGameKeyHandler
    {
        /// <summary>
        /// Process a game key input. Return true if handled, false to pass through.
        /// </summary>
        bool HandleGameKey(GameKey key);
    }
}