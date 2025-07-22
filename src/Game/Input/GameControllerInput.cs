using Orbit.Input;

namespace Breakout.Game.Input;

public class GameControllerInput : IInputController
{
    private readonly BreakoutGame _game;
    private Orbit.Input.GameController _gameController;
    
    public GameControllerInput(BreakoutGame game)
    {
        _game = game;
        
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
        
        if (_gameController.LeftStick.XAxis.Value < -0.001f)
        {
            _game.ApplyGameKey(GameKey.Left);
        }
        else if (_gameController.LeftStick.XAxis.Value > 0.001f)
        {
            _game.ApplyGameKey(GameKey.Right);
        }
        else
        {
            _game.ApplyGameKey(GameKey.Stop);
        }
        
        if (_gameController.LeftStick.YAxis.Value < -0.001f)
        {
            _game.ApplyGameKey(GameKey.Down);
        }
        else if (_gameController.LeftStick.YAxis.Value > 0.001f)
        {
            _game.ApplyGameKey(GameKey.Up);
        }

        if (_gameController.South.Value)
        {
            _game.ApplyGameKey(GameKey.Fire);
        }
        
        if (_gameController.Pause.Value)
        {
            _game.ApplyGameKey(GameKey.Pause);
        }
    }

    public void Dispose()
    {
        _game?.Dispose();
        
        GameControllerManager.Current.GameControllerConnected -= OnGameControllerConnected;
    }
}