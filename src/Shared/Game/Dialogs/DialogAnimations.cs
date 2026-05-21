namespace Breakout.Game.Dialogs;

/// <summary>
/// Animation definitions for dialog appearance and disappearance
/// </summary>
public class DialogAnimations
{
    public Func<SkiaLayout, CancellationToken, Task> BackdropAppearing { get; set; }
    public Func<SkiaLayout, CancellationToken, Task> BackdropDisappearing { get; set; }
    public Func<SkiaLayout, CancellationToken, Task> FrameAppearing { get; set; }
    public Func<SkiaLayout, CancellationToken, Task> FrameDisappearing { get; set; }
}