namespace Breakout.Game.Dialogs;

/// <summary>
/// Template system for customizing dialog appearance and behavior
/// </summary>
public class DialogTemplate
{
    public Func<GameDialog, SkiaControl, string, string, SkiaLayout> CreateDialogFrame { get; set; }
    public Func<SkiaLayout> CreateBackdrop { get; set; }
    public Func<string, SkiaControl> CreateButton { get; set; }
    public DialogAnimations Animations { get; set; }
}