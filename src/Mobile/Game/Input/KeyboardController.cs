namespace Breakout.Game.Input;

public class KeyboardController : IInputController
{
    private BreakoutGame _game;
    private readonly Dictionary<GameKey, KeyState> _keyStates;

    // Movement keys that need tracking to send Stop when all released
    private static readonly HashSet<GameKey> DirectionalKeys = new()
    {
        GameKey.Left,
        GameKey.Right,
        GameKey.Up,
        GameKey.Down
    };

    public KeyboardController(BreakoutGame game)
    {
        _game = game;
        _keyStates = new Dictionary<GameKey, KeyState>
        {
            { GameKey.Left, KeyState.Released },
            { GameKey.Right, KeyState.Released },
            { GameKey.Up, KeyState.Released },
            { GameKey.Down, KeyState.Released }
        };
    }

    public void Dispose()
    {
        _game = null;
        _keyStates.Clear();
    }

    public void ProcessState()
    {
        var hasWillRelease = false;
        var hasPressed = false;

        // Check if any key is being released or is pressed
        foreach (var kvp in _keyStates)
        {
            if (kvp.Value == KeyState.WillRelease)
            {
                hasWillRelease = true;
            }
            if (kvp.Value == KeyState.Pressed)
            {
                hasPressed = true;
            }
        }

        // Send stop if we have keys that will be released and no keys are currently pressed
        if (hasWillRelease && !hasPressed)
        {
            _game.SendKey(GameKey.Stop);

            // Update all WillRelease states to Released
            var keysToUpdate = _keyStates.Where(kvp => kvp.Value == KeyState.WillRelease)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToUpdate)
            {
                _keyStates[key] = KeyState.Released;
            }
        }
        else
        {
            // Send all currently pressed keys
            foreach (var kvp in _keyStates)
            {
                if (kvp.Value == KeyState.Pressed)
                {
                    _game.SendKey(kvp.Key);
                }
            }
        }
    }

    public void SetKeyPressed(GameKey key, bool isPressed)
    {
        // Directional keys use state tracking
        if (DirectionalKeys.Contains(key))
        {
            if (isPressed)
            {
                _keyStates[key] = KeyState.Pressed;
            }
            else
            {
                // Key released - only set to WillRelease if it was pressed
                if (_keyStates[key] == KeyState.Pressed)
                {
                    _keyStates[key] = KeyState.WillRelease;
                }
            }
        }
        else
        {
            // Action keys (Fire, Pause, etc.) are sent immediately on press
            if (isPressed)
            {
                _game.SendKey(key);
            }
        }
    }
}