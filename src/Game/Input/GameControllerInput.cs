using Orbit.Input;

namespace Breakout.Game.Input;

public class GameControllerInput : IInputController
{
    private readonly BreakoutGame _game;
    private Orbit.Input.GameController _gameController;
    private bool _wasMoving;
    private readonly Dictionary<GameKey, bool> _buttonStates;

    public GameControllerInput(BreakoutGame game)
    {
        _game = game;
        _wasMoving = false;
        _buttonStates = new Dictionary<GameKey, bool>
        {
            { GameKey.Fire, false },
            { GameKey.Pause, false }
        };

        GameControllerManager.Current.GameControllerConnected += OnGameControllerConnected;

        _ = GameControllerManager.Current.StartDiscovery();
    }

    private void OnGameControllerConnected(object sender, GameControllerConnectedEventArgs args)
    {
        _gameController = args.GameController;
    }

    public void ProcessState()
    {
        if (_gameController is null)
        {
            return;
        }

        // Handle directional input
        var isMoving = false;

        if (_gameController.LeftStick.XAxis.Value < -0.001f ||
            _gameController.Dpad.XAxis.Value < -0.001f)
        {
            _game.SendKey(GameKey.Left);
            isMoving = true;
        }
        else if (_gameController.LeftStick.XAxis.Value > 0.001f ||
                 _gameController.Dpad.XAxis.Value > 0.001f)
        {
            _game.SendKey(GameKey.Right);
            isMoving = true;
        }

        if (_gameController.LeftStick.YAxis.Value < -0.001f ||
            _gameController.Dpad.YAxis.Value < -0.001f)
        {
            _game.SendKey(GameKey.Up);
            isMoving = true;
        }
        else if (_gameController.LeftStick.YAxis.Value > 0.001f ||
                 _gameController.Dpad.YAxis.Value > 0.001f)
        {
            _game.SendKey(GameKey.Down);
            isMoving = true;
        }

        // Only send Stop when transitioning from moving to stopped
        if (!isMoving && _wasMoving)
        {
            _game.SendKey(GameKey.Stop);
        }

        _wasMoving = isMoving;

        // Handle action buttons - only fire once per press
        HandleButton(GameKey.Fire, _gameController.South.Value);
        HandleButton(GameKey.Pause, _gameController.Pause.Value);
    }

    private void HandleButton(GameKey gameKey, bool isPressed)
    {
        if (isPressed && !_buttonStates[gameKey])
        {
            _game.SendKey(gameKey);
            _buttonStates[gameKey] = true;
        }
        else if (!isPressed)
        {
            _buttonStates[gameKey] = false;
        }
    }

    public void Dispose()
    {
        _game?.Dispose();
        _buttonStates.Clear();

        GameControllerManager.Current.GameControllerConnected -= OnGameControllerConnected;
    }
}