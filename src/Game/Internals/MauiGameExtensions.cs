using SkiaSharp;

namespace Breakout.Game;

public static class MauiGameExtensions
{

    public static SKRect GetHitBox(this SkiaControl sprite)
    {
        if (sprite is SkiaControl control)
        {
            //real position even below/inside a cached layer
            //var position = control.GetFuturePositionOnCanvasInPoints();
            if (control.VisualLayer == null)
            {
                return SKRect.Empty;
            }

            //we can get position from layer as we do not ache above
            var position = control.VisualLayer.HitBoxWithTransforms.Units.Location;

            var hitBox = new SKRect(position.X, position.Y,
                (float)(position.X + control.Width), (float)(position.Y + control.Height));
            return hitBox;
        }

        return SKRect.Empty;
    }

    /// <summary>
    /// Detects intersection between two rectangles and returns the overlap area
    /// </summary>
    /// <param name="source">Source rectangle</param>
    /// <param name="target">Target rectangle</param>
    /// <param name="overlap">Output overlap rectangle</param>
    /// <returns>True if rectangles intersect, false otherwise</returns>
    public static bool IntersectsWith(this SKRect source, SKRect target, out SKRect overlap)
    {
        // Initialize overlap with empty rect
        overlap = SKRect.Empty;

        // Calculate potential overlap dimensions
        float left = Math.Max(source.Left, target.Left);
        float right = Math.Min(source.Right, target.Right);
        float top = Math.Max(source.Top, target.Top);
        float bottom = Math.Min(source.Bottom, target.Bottom);

        // Check if there's actually an overlap
        if (right <= left || bottom <= top)
        {
            return false;
        }

        // Create overlap rectangle relative to target position
        overlap = new SKRect(
            left - target.Left, // Normalize to target's left edge
            top - target.Top, // Normalize to target's top edge
            right - target.Left, // Normalize to target's left edge
            bottom - target.Top // Normalize to target's top edge
        );

        return true;
    }


}