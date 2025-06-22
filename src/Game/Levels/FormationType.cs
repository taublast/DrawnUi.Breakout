using SkiaSharp;
using System.Numerics;

namespace BreakoutGame.Game
{
    /// <summary>
    /// Represents a formation type for brick layouts
    /// </summary>
    public enum FormationType
    {
        Grid,           // Regular grid layout
        Pyramid,        // Triangular/pyramid shape
        Arch,           // Arch or multiple arches
        Diamond,        // Diamond/rhombus pattern
        Zigzag,         // Zigzag pattern
        Spiral,         // Spiral formation
        Organic,        // Organic/irregular shape using noise
        Wave            // Wave pattern
    }
}