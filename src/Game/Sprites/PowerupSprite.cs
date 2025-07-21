using SkiaSharp;

namespace Breakout.Game;

public class PowerUpSprite : SkiaShape, IWithHitBox, IReusableSprite
{
    public static float FallSpeed = BreakoutGame.POWERUP_SPEED;

    private SkiaLabel _letterLabel;

    public static PowerUpSprite Create()
    {
        var powerup = new PowerUpSprite();
        powerup.Initialize();
        return powerup;
    }

    private void Initialize()
    {
        UseCache = SkiaCacheType.Operations;
        WidthRequest = 34;
        HeightRequest = 18;
        CornerRadius = 4;
        BackgroundColor = Colors.Purple;
        StrokeColor = Colors.White;
        StrokeWidth = 2;
        SpeedRatio = 1;
        IsActive = false;
        ZIndex = 3;

        _letterLabel = new SkiaLabel
        {
            Text = "?",
            UseCache = SkiaCacheType.Operations,
            FontSize = 10,
            TextColor = Colors.White,
            FontFamily = "FontSystem",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        Children = new List<SkiaControl> { _letterLabel };
    }

    public PowerupType Type { get; set; }
    public bool IsActive { get; set; }
    public float SpeedRatio { get; set; }

    public void SetPowerupType(PowerupType type)
    {
        Type = type;

        // Set letter and color based on type
        switch (type)
        {
            case PowerupType.Destroyer:
                _letterLabel.Text = "A";
                BackgroundColor = Colors.Red;
                _letterLabel.TextColor = WhiteColor;
                break;
            case PowerupType.ExpandPaddle:
                _letterLabel.Text = "E";
                BackgroundColor = Colors.BlueViolet;
                _letterLabel.TextColor = WhiteColor;
                break;
            case PowerupType.StickyBall:
                _letterLabel.Text = "C";
                BackgroundColor = Colors.Cyan;
                _letterLabel.TextColor = BlackColor;
                break;
            case PowerupType.ShrinkPaddle:
                _letterLabel.Text = "R";
                BackgroundColor = Colors.Orange;
                _letterLabel.TextColor = BlackColor;
                break;
            case PowerupType.SlowBall:
                _letterLabel.Text = "S";
                BackgroundColor = Colors.White;
                _letterLabel.TextColor = BlackColor;
                break;
            case PowerupType.FastBall:
                _letterLabel.Text = "F";
                BackgroundColor = Colors.CornflowerBlue;
                _letterLabel.TextColor = BlackColor;
                break;
            case PowerupType.MultiBall:
                _letterLabel.Text = "M";
                BackgroundColor = Colors.Magenta;
                _letterLabel.TextColor = WhiteColor;
                break;
            case PowerupType.ExtraLife:
                _letterLabel.Text = "L";
                BackgroundColor = Colors.GreenYellow;
                _letterLabel.TextColor = BlackColor;
                break;

            default:
                _letterLabel.Text = "?";
                BackgroundColor = Colors.White;
                _letterLabel.TextColor = BlackColor;
                break;
        }

        StrokeColor = BackgroundColor.MakeLighter(50f);
    }

    public void UpdateRotation(float deltaTime)
    {
        // Rotate the letter for visual effect
        if (_letterLabel != null)
        {
            LetterRotation += 180 * deltaTime; // Rotate 180 degrees per second
            if (LetterRotation > 360)
                LetterRotation -= 360;
        }
    }

    double LetterRotation
    {
        get { return _letterLabel.RotationX; }
        set { _letterLabel.RotationX = value; }
    }

    public void ResetAnimationState()
    {
        Opacity = 1;
        Scale = 1;
        if (_letterLabel != null)
            LetterRotation = 0;
    }

    public async Task AnimateDisappearing()
    {
        await FadeToAsync(0, 150);
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