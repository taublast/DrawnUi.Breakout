using AppoMobi.Maui.Gestures;
using Breakout.Game.Input;

namespace Breakout.Game.Controls;

public class SelectableGameButton : SkiaShape, IGameKeyHandler
{
    public bool HandleGameKey(GameKey key)
    {
        if (key == GameKey.Fire)
        {
            var tap = new SkiaGesturesParameters()
            {
                Type = TouchActionResult.Tapped
            };
            ProcessGestures(tap, GestureEventProcessingInfo.Empty);
            return true;
        }

        return false;
    }

 
}