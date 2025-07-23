namespace Breakout.Game;

public class GameSwitch : SkiaSwitch
{
    public GameSwitch()
    {
        WidthRequest = 60;
        HeightRequest = 32;
        ColorFrameOff = BreakoutGame.UiElements.ColorIconSecondary;
        ColorFrameOn = BreakoutGame.UiElements.ColorPrimary;
        ColorThumbOff = AmstradColors.White;
        ColorThumbOn = AmstradColors.White;
        UseCache = SkiaCacheType.Operations;
    }
}