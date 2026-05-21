using SkiaSharp;

namespace Breakout.Game;

public interface IWithHitBox
{
    /// <summary>
    /// Calculate hitbox etc for the current frame
    /// </summary>
    /// <param name="time"></param>
    void UpdateState(long time, bool forceRecalculate=false);

    /// <summary>
    /// Precalculated
    /// </summary>
    SKRect HitBox { get; }
}