using SkiaSharp;

namespace Breakout.Game;

public enum GameState
{
    Unset,

    /// <summary>
    /// Welcome screen presented
    /// </summary>
    Ready,

    /// <summary>
    /// Game loop is running
    /// </summary>
    Playing,

    Paused,

    /// <summary>
    /// Game ended
    /// </summary>
    Ended,

    LevelComplete,
    
    DemoPlay
}

public enum PowerupType
{
    None,
    Destroyer,
    WidePaddle,
    ExtraLife,
    StickyBall,
    SlowBall
}

public class PowerupSprite : SkiaShape, IWithHitBox, IReusableSprite
{
    public static PowerupSprite Create()
    {
        return new PowerupSprite
        {
            BackgroundColor = Colors.Purple,
            CornerRadius = 20,
            WidthRequest = 20,
            HeightRequest = 20,
            UseCache = SkiaCacheType.Image,
            ZIndex = 3
        };
    }

    public PowerupType Type { get; set; }
    public bool IsActive { get; set; }
    public float FallSpeed { get; set; } = 100f;

    public void ResetAnimationState()
    {
        Opacity = 1;
        Scale = 1;
    }

    public async Task AnimateDisappearing()
    {
        await FadeToAsync(0, 150);
    }

    public void UpdateState(long time)
    {
        if (_stateUpdated != time)
        {
            HitBox = this.GetHitBox();
            _stateUpdated = time;
        }
    }
    private long _stateUpdated;

    public SKRect HitBox { get; private set; }
}