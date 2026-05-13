namespace Breakout.Game.Input;

public interface IInputController : IDisposable
{
    void ProcessState();
}