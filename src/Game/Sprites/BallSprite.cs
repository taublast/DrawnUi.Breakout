using System.Numerics;
using SkiaSharp;

namespace Breakout.Game;

public class BallSprite : SkiaShape, IWithHitBox//, IReusableSprite
{
    public override void InvalidateCache()
    {
        //base.InvalidateCache(); - disable cache invalidation, we will need it built only once
    }

    //BackgroundColor="#dddddd"
    public BallSprite()
    {
        UseCache = SkiaCacheType.GPU;
        HeightRequest = 15;
        HorizontalOptions = LayoutOptions.Center;
        VerticalOptions = LayoutOptions.End;
        Type = ShapeType.Circle;
        StrokeColor = Colors.White;
        StrokeWidth = 2;
        LockRatio = 1;
        BackgroundColor = Colors.Aqua;
        SpeedRatio = 1;
        IsActive = true;
        BevelType = BevelType.Bevel;
        Bevel = new SkiaBevel()
        {
            Depth = 4,
            LightColor = Colors.White,
            ShadowColor = Color.Parse("#333333"),
            Opacity = 0.33
        };
    }

    public static BallSprite Create()
    {
        return new();
    }

    public bool IsActive { get; set; }

    public float Angle
    {
        get => _angle;
        set { _angle = ClampAngleFromHorizontal(value); }
    }


    public float SpeedRatio { get; set; }

    public bool IsMoving
    {
        get => _isMoving;
        set
        {
            if (value == _isMoving) return;
            _isMoving = value;
            OnPropertyChanged();
        }
    }

    public void ResetAnimationState()
    {
    }

    public async Task AnimateDisappearing()
    {
    }

    public void UpdateState(long time, bool force=false)
    {
        if (force || _stateUpdated != time)
        {
            HitBox = this.GetHitBox();
            _stateUpdated = time;
        }
    }

    long _stateUpdated;
    public SKRect HitBox { get; set; }

    // New interpolation fields
    private float _lastMoveX;
    private float _lastMoveY;
    private float _interpolationFactor = 0.5f; // Smoothing factor
    private float _angle;
    private bool _isMoving;

    /// <summary>
    /// Change the current position offset by the provided amount in points
    /// </summary>
    public void MoveOffset(double x, double y)
    {
        Left += x;
        Top += y;
        //Repaint();
    }

    /// <summary>
    /// Replaces the current position offset by the provided amount in points
    /// </summary>
    public void SetOffset(double x, double y)
    {
        Left = x;
        Top = y;
        //Repaint();
    }

    public void SetOffsetX(double x)
    {
        Left = x;
        //Repaint();
    }

    public void SetOffsetY(double y)
    {
        Top = y;
        //Repaint();
    }

    public void UpdatePosition(float deltaSeconds)
    {
        // Ensure delta is capped and positive
        if (deltaSeconds <= 0 || !IsMoving)
            return;

        // Calculate new movement
        float moveX = BreakoutGame.BALL_SPEED * SpeedRatio * MathF.Cos(Angle) * deltaSeconds;
        float moveY = BreakoutGame.BALL_SPEED * SpeedRatio * MathF.Sin(Angle) * deltaSeconds;

        //// Interpolate movement for smoother transition
        //moveX = float.Lerp(_lastMoveX, moveX, _interpolationFactor);
        //moveY = float.Lerp(_lastMoveY, moveY, _interpolationFactor);
        //_lastMoveX = moveX;
        //_lastMoveY = moveY;

        // Apply interpolated movement
        Left += moveX;
        Top += moveY;

        //Repaint();
    }

    /// <summary>
    /// Returns the current movement direction as a normalized vector
    /// </summary>
    public Vector2 Direction => new Vector2(MathF.Cos(Angle), MathF.Sin(Angle));

    /// <summary>
    /// Returns the ball's center position as a Vector2
    /// </summary>
    public Vector2 Position => new Vector2(HitBox.MidX, HitBox.MidY);

    /// <summary>
    /// Sets the ball's direction from a Vector2 (automatically normalizes)
    /// </summary>
    public void SetDirection(Vector2 direction)
    {
        if (direction == Vector2.Zero)
            return;

        Vector2 normalized = Vector2.Normalize(direction);
        Angle = MathF.Atan2(normalized.Y, normalized.X);
    }

    /// <summary>
    /// Clamps an angle (in radians) to ensure it is at least a minimum deviation
    /// away from the horizontal axes (0, +/- PI, +/- 2*PI, etc.).
    /// </summary>
    /// <param name="angle">The input angle in radians.</param>
    /// <param name="minAngleFromHorizontal">
    /// The minimum desired angle (in radians) between the output angle and the
    /// nearest horizontal axis. Must be positive and less than PI/2 (90 degrees).
    /// </param>
    /// <returns>
    /// The adjusted angle (in radians), guaranteed to be at least minAngleFromHorizontal
    /// away from any horizontal axis. The returned angle will be normalized to the range (-PI, PI].
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if minAngleFromHorizontal is not within the valid range (0, PI/2).
    /// </exception>
    public static float ClampAngleFromHorizontal(float angle, float minAngleFromHorizontal = MathF.PI / 10.0f)
    {
        // --- Input Validation ---
        if (minAngleFromHorizontal <= 0 || minAngleFromHorizontal >= MathF.PI / 2.0f)
        {
            throw new ArgumentOutOfRangeException(nameof(minAngleFromHorizontal),
                "Must be positive and less than PI/2 radians.");
        }

        // --- Normalize angle to the range (-PI, PI] for easier comparison ---
        // This simplifies checking against 0 and +/- PI
        float twoPi = 2.0f * MathF.PI;
        float normalizedAngle = angle % twoPi; // Bring angle within (-2*PI, 2*PI)

        // Adjust to (-PI, PI] range
        if (normalizedAngle <= -MathF.PI)
        {
            normalizedAngle += twoPi;
        }
        else if (normalizedAngle > MathF.PI)
        {
            normalizedAngle -= twoPi;
        }
        // Now normalizedAngle is guaranteed to be in the interval (-PI, PI]

        // --- Check if the angle is too close to horizontal ---
        bool tooCloseToZero = MathF.Abs(normalizedAngle) < minAngleFromHorizontal;
        bool tooCloseToPi = MathF.Abs(normalizedAngle) > (MathF.PI - minAngleFromHorizontal);

        if (tooCloseToZero || tooCloseToPi)
        {
            // --- Adjust the angle ---
            // Determine the sign to preserve the general direction (up/down)
            float sign = MathF.Sign(normalizedAngle);

            // Handle the edge case where normalizedAngle is exactly 0
            if (sign == 0)
            {
                // If the original angle was 0 or a multiple of 2*PI, it normalized to 0.
                // We need a consistent direction to nudge it. Let's default to positive (up/right quadrant).
                // You might adjust this based on game context if 0 needs special handling.
                sign = 1;
            }

            if (tooCloseToZero)
            {
                // If it was near 0, set it to the minimum angle away from 0
                return sign * minAngleFromHorizontal;
            }
            else // tooCloseToPi
            {
                // If it was near +/- PI, set it to the minimum angle away from +/- PI
                return sign * (MathF.PI - minAngleFromHorizontal);
            }
        }
        else
        {
            // Angle is not too horizontal, return the normalized version
            return normalizedAngle;
        }
    }

}