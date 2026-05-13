using System.Diagnostics;
using SkiaSharp;

namespace Breakout.Game.Input;

public class HudController : IInputController
{
    private readonly BreakoutGame _game;
    private bool _leftPressed = false;
    private bool _rightPressed = false;
    private Dictionary<long, bool> _touchSides = new Dictionary<long, bool>(); // true = left, false = right

    public HudController(BreakoutGame game)
    {
        _game = game;
    }

    public bool ProcessGestures(SkiaGesturesParameters args, GestureEventProcessingInfo apply, SKRect hitbox,
        float scale)
    {
        if (!_game.InputPressMode)
            return false;

        var point = new SKPoint(args.Event.Location.X / scale, args.Event.Location.Y / scale);

        if (!hitbox.Contains(point))
            return false;

        bool isLeftSide = point.X < hitbox.Left + hitbox.Width / 2;

        if (args.Type == TouchActionResult.Down)
        {
            _touchSides[args.Event.Id] = isLeftSide;

            if (isLeftSide)
            {
                Debug.WriteLine("DOWN LEFT");
                _leftPressed = true;
            }
            else
            {
                Debug.WriteLine("DOWN RIGHT");
                _rightPressed = true;
            }

            return true;
        }
        else if (args.Type == TouchActionResult.Up)
        {
            if (_touchSides.TryGetValue(args.Event.Id, out bool wasLeftSide))
            {
                _touchSides.Remove(args.Event.Id);

                if (wasLeftSide)
                {
                    Debug.WriteLine("UP LEFT");
                    _leftPressed = false;
                }
                else
                {
                    Debug.WriteLine("UP RIGHT");
                    _rightPressed = false;
                }
            }

            if (!_leftPressed && !_rightPressed)
            {
                _game.SendKey(GameKey.Stop);
            }

            return true;
        }

        return false;
    }

    public void ProcessState()
    {
        if (!_game.InputPressMode)
            return;

        if (_rightPressed && _leftPressed)
        {
            _game.SendKey(GameKey.Right);
        }
        else if (_leftPressed)
        {
            _game.SendKey(GameKey.Left);
        }
        else if (_rightPressed)
        {
            _game.SendKey(GameKey.Right);
        }
    }

    public void Reset()
    {
        _leftPressed = false;
        _rightPressed = false;
        _touchSides.Clear();
    }

    public void Dispose()
    {
    }
}
