using System.Diagnostics;
using AppoMobi.Maui.Gestures;
using SkiaSharp;

namespace Breakout.Game.Input;

public class HudController : IInputController
{
    private readonly BreakoutGame _game;
    private bool _leftPressed = false;
    private bool _rightPressed = false;
    private Dictionary<long, bool> _touchSides = new Dictionary<long, bool>(); // true = left, false = right

    /// <summary>
    /// Initializes a new instance of the VirtualController class
    /// </summary>
    /// <param name="game">The breakout game instance</param>
    public HudController(BreakoutGame game)
    {
        _game = game;
    }

    /// <summary>
    /// Processes touch gestures and updates internal key state
    /// </summary>
    /// <param name="args">The gesture parameters</param>
    /// <param name="apply">The gesture processing info</param>
    /// <param name="hitbox">The hit box area for touch detection</param>
    /// <param name="scale">The rendering scale factor</param>
    /// <returns>True if gesture was consumed, false otherwise</returns>
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

            return true;
        }

        return false;
    }

    /// <summary>
    /// Called every frame to apply continuous key input based on current state
    /// </summary>
    public void ProcessState()
    {
        if (!_game.InputPressMode)
            return;

        if (_rightPressed && _leftPressed)
        {
            // Both pressed - prioritize the most recent one
            // For now, right takes priority when both are pressed
            _game.ApplyGameKey(GameKey.Right);
        }
        else if (_leftPressed)
        {
            _game.ApplyGameKey(GameKey.Left);
        }
        else if (_rightPressed)
        {
            _game.ApplyGameKey(GameKey.Right);
        }
    }

    /// <summary>
    /// Resets the virtual controller state
    /// </summary>
    public void Reset()
    {
        _leftPressed = false;
        _rightPressed = false;
        _touchSides.Clear();
    }
}