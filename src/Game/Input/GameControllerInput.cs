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
            _game.SendKey(GameKey.Left);
        }
        else if (_gameController.LeftStick.XAxis.Value > 0.001f)
        {
            _game.SendKey(GameKey.Right);
        }
        else
        {
            _game.SendKey(GameKey.Stop);
        }
        
        if (_gameController.LeftStick.YAxis.Value < -0.001f)
        {
            _game.SendKey(GameKey.Down);
        }
        else if (_gameController.LeftStick.YAxis.Value > 0.001f)
        {
            _game.SendKey(GameKey.Up);
        }

        if (_gameController.South.Value)
        {
            _game.SendKey(GameKey.Fire);
        }
        
        if (_gameController.Pause.Value)
        {
            _game.SendKey(GameKey.Pause);
        }
    }

    public void Dispose()
    {
        _game?.Dispose();
        
        GameControllerManager.Current.GameControllerConnected -= OnGameControllerConnected;
    }
}