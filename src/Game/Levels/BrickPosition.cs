namespace Breakout.Game;

/// <summary>
/// Represents a position for a brick in the layout
/// </summary>
public class BrickPosition
{
    /// <summary>
    /// Column index
    /// </summary>
    public float Column { get; set; }

    /// <summary>
    /// Row index
    /// </summary>
    public float Row { get; set; }

    /// <summary>
    /// Preset ID to use for this brick
    /// </summary>
    public string PresetId { get; set; }
}