using SkiaSharp;

namespace Breakout.Game;

public class PaddleSprite : SkiaShape, IWithHitBox //, IReusableSprite
{
    public static float Speed = BreakoutGame.PADDLE_SPEED;

    Color ColorA = Colors.DarkOrange;
    private Color ColorB = AmstradColors.BrightGreen;

    public PaddleSprite()
    {
        UseCache = SkiaCacheType.GPU;
        HeightRequest = 16;
        CornerRadius = 8;
        BackgroundColor = ColorA;
        HorizontalOptions = LayoutOptions.Center;
        VerticalOptions = LayoutOptions.End;
        Type = ShapeType.Rectangle;
        StrokeColor = Color.Parse("#CCCCFF");
        StrokeWidth = 2;
        SpeedRatio = 1;
        IsActive = true;
        BevelType = BevelType.Bevel;
        Bevel = new SkiaBevel()
        {
            Depth = 4,
            LightColor = Colors.White,
            ShadowColor = Color.Parse("#333333"),
            Opacity = 0.33,
        };
        Children = new List<SkiaControl>()
            {
                new SkiaShape()
                {
                    CornerRadius = 2,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill,
                    BackgroundColor = ColorB,
                    Margin = new(14, 3),
                    BevelType = BevelType.Emboss,
                    Bevel = new SkiaBevel()
                    {
                        Depth = 2,
                        LightColor = Colors.White,
                        ShadowColor = Color.Parse("#333333"),
                        Opacity = 0.33,
                    }
                }
            }
            ;

        ApplyPowerup(PowerupType.None);

        //Powerup = PowerupType.StickyBall;
    }

    public float Angle { get; set; }

    private PowerupType _powerup;

    public PowerupType Powerup
    {
        get { return _powerup; }
        set
        {
            if (value != _powerup)
            {
                _powerup = value;
                ApplyPowerup(value);
            }
        }
    }

    public float PowerupDuration { get; set; }

    /// <summary>
    /// Update look upon powerups
    /// </summary>
    protected void ApplyPowerup(PowerupType powerup)
    {
        //DURATION
        if (powerup == PowerupType.None)
        {
            PowerupDuration = 0;
        }
        else
        {
            PowerupDuration = BreakoutGame.POWERUP_DURATION;
        }

        // Remove incorrect speed logic - ball speed should be handled in BreakoutGame
        // SpeedRatio should remain 1 for paddle

        //SIZE
        if (Powerup == PowerupType.ExpandPaddle)
        {
            WidthRequest = BreakoutGame.PADDLE_WIDTH * 1.33;
        }
        else
        {
            WidthRequest = BreakoutGame.PADDLE_WIDTH;
        }

        //COLORS
        if (Powerup == PowerupType.Destroyer)
        {
            BackgroundColor = AmstradColors.Red;
        }
        else
        if (Powerup == PowerupType.StickyBall)
        {
            BackgroundColor = Colors.DarkCyan;
        }
        else
        {
            BackgroundColor = ColorA;
        }
    }

    public bool IsActive { get; set; }

    public static PaddleSprite Create()
    {
        return new();
    }

    public void ResetAnimationState()
    {
    }

    public async Task AnimateDisappearing()
    {
    }

    public void UpdateState(long time, bool force = false)
    {
        if (force || _stateUpdated != time)
        {
            HitBox = this.GetHitBox();
            _stateUpdated = time;
        }
    }

    long _stateUpdated;
    public SKRect HitBox { get; set; }
    public float SpeedRatio { get; set; }

    public void UpdatePosition(float deltaTime)
    {
        // we are not updating it here, but in the game loop
    }
}
