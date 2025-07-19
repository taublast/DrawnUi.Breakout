using SkiaSharp;

namespace Breakout.Game;

public class PaddleBulletSprite : SkiaShape, IWithHitBox, IReusableSprite
{
    public static float Speed = 400f;

    public static PaddleBulletSprite Create()
    {
        var bullet = new PaddleBulletSprite()
        {
            HeightRequest = 12,
            WidthRequest = 3,
            CornerRadius = 2,
            BackgroundColor = Color.Parse("#ffff4444"),
            StrokeColor = Color.Parse("#ffff0000"),
            StrokeWidth = 1,
            UseCache = SkiaCacheType.Operations,
            SpeedRatio = 1,
            IsActive = false,
            ZIndex = 2
        };
        return bullet;
    }

    public bool IsActive { get; set; }
    public float SpeedRatio { get; set; }

    public void ResetAnimationState()
    {
        Opacity = 1;
        Scale = 1;
    }

    public async Task AnimateDisappearing()
    {
        await FadeToAsync(0, 100);
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