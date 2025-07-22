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

        _gameController.ButtonChanged += GameControllerOnButtonChanged;
        _gameController.ValueChanged += GameControllerOnValueChanged;
    }

    private void GameControllerOnValueChanged(object sender, GameControllerValueChangedEventArgs e)
    {
        
    }

    private void GameControllerOnButtonChanged(object sender, GameControllerButtonChangedEventArgs e)
    {
        
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
    }
}