using AppoMobi.Maui.Gestures;
using Breakout.Game.Input;

namespace Breakout.Game;

/// <summary>
/// To support selected options inside dialog
/// </summary>
public class OptionWithTappable : SkiaLayout, IGameKeyHandler
{
    private readonly string _tag;
    private readonly SkiaControl _control;

    public OptionWithTappable(SkiaControl control)
    {
        _control = control;
    }

    public OptionWithTappable(string tag)
    {
        _tag = tag;
    }

    public bool HandleGameKey(GameKey key)
    {
        if (key == GameKey.Fire)
        {
            var control = _control;
            if (control == null)
            {
                control = FindViewByTag(_tag);
            }
            if (control != null)
            {
                var tap = new SkiaGesturesParameters()
                {
                    Type = TouchActionResult.Tapped
                };
                control.ProcessGestures(tap, GestureEventProcessingInfo.Empty);
            }
            return true;
        }

        return false;
    }
    public bool IsSelected { get; set; }
}