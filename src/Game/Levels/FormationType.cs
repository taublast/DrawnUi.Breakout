namespace Breakout.Game
{
    /// <summary>
    /// Represents a formation type for brick layouts
    /// </summary>
    public enum FormationType
    {
        Grid,           // Regular grid layout
        Pyramid,        // Triangular/pyramid shape
        Arch,           // Arch or multiple arches
        Diamond,
        Zigzag,
        Organic,        // Organic/irregular shape using noise
        Wave,
        Maze,           // Maze-like corridors and walls
    }
}