using SkiaSharp;

namespace Breakout.Game;

public class BulletSprite : SkiaLayout, IWithHitBox, IReusableSprite
{
    public override void InvalidateCache()
    {
        //base.InvalidateCache(); - disable cache invalidation, we will need it built only once
    }

    protected override void OnCacheDestroyed()
    {
        base.OnCacheDestroyed();
    }


    public static float Speed = 500f;

    public static BulletSprite Create()
    {
        return new BulletSprite
        {
            Tag = "Bullet",
            WidthRequest = BreakoutGame.PADDLE_WIDTH,
            UseCache = SkiaCacheType.Operations,
            SpeedRatio = 1,
            ZIndex = 2,
            HeightRequest = 16,
            //same as player
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.End,
            Children =
            {
                CreateLazer(),
                CreateLazer().EndX(),
            }
        };
    }

    static SkiaShape CreateLazer()
    {
        return new SkiaShape()
        {
            WidthRequest = 5,
            CornerRadius = 2,
            BackgroundColor = Color.Parse("#ffff4444"),
            StrokeColor = Color.Parse("#ffff0000"),
            StrokeWidth = 1,
            VerticalOptions = LayoutOptions.Fill,
        };
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

    public void UpdateState(long time, bool force = false)
    {
        if (force || _stateUpdated != time)
        {
            HitBox = this.GetHitBox();
            _stateUpdated = time;
        }
    }

    private long _stateUpdated;
    public SKRect HitBox { get; private set; }
}