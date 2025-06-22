using SkiaSharp;

namespace BreakoutGame.Game;

public interface IWithHitBox
{
    /// <summary>
    /// Calculate hitbox etc for the current frame
    /// </summary>
    /// <param name="time"></param>
    void UpdateState(long time);

    /// <summary>
    /// Precalculated
    /// </summary>
    SKRect HitBox { get; }
}