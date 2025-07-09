using SkiaSharp;

namespace Breakout.Game;

/// <summary>
/// Extension methods for the Breakout game
/// </summary>
public static class BreakoutGameExtensions
{
    /// <summary>
    /// Identifies the collision face based on overlap rectangle
    /// </summary>
    /// <param name="overlap">The overlap rectangle</param>
    /// <param name="targetRect">The target rectangle</param>
    /// <returns>The collision face (Top, Bottom, Left, Right)</returns>
    public static CollisionFace GetCollisionFace(this SKRect overlap, SKRect targetRect)
    {
        // Calculate penetration depths for each edge
        float leftPenetration = overlap.Right;
        float rightPenetration = targetRect.Width - overlap.Left;
        float topPenetration = overlap.Bottom;
        float bottomPenetration = targetRect.Height - overlap.Top;

        // Find minimum penetration
        float minPenetration = MathF.Min(
            MathF.Min(leftPenetration, rightPenetration),
            MathF.Min(topPenetration, bottomPenetration)
        );

        // Return face with minimum penetration
        if (minPenetration == leftPenetration)
            return CollisionFace.Left;
        else if (minPenetration == rightPenetration)
            return CollisionFace.Right;
        else if (minPenetration == topPenetration)
            return CollisionFace.Top;
        else
            return CollisionFace.Bottom;
    }
}