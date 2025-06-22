namespace BreakoutGame.Game;

/// <summary>
/// Game-dependent action keys
/// </summary>
public enum GameKey
{
    Unset,
    Demo,
    Pause,
    Fire,
    Left,
    Right,
    Stop
}

public enum EventType
{
    KeyDown,
    KeyUp,
    Mouse,
    Joystick
}

public static class AmstradColors
{
    public static readonly Color Black = Color.Parse("#000000");
    public static readonly Color Blue = Color.Parse("#0000FF");
    public static readonly Color Red = Color.Parse("#FF0000");
    public static readonly Color Magenta = Color.Parse("#FF00FF");
    public static readonly Color Green = Color.Parse("#00FF00");
    public static readonly Color Cyan = Color.Parse("#00FFFF");
    public static readonly Color Yellow = Color.Parse("#FFFF00");
    public static readonly Color White = Color.Parse("#FFFFFF");
    public static readonly Color Grey = Color.Parse("#808080");
    public static readonly Color BrightBlue = Color.Parse("#0080FF");
    public static readonly Color BrightRed = Color.Parse("#FF8080");
    public static readonly Color BrightMagenta = Color.Parse("#FF80FF");
    public static readonly Color BrightGreen = Color.Parse("#80FF80");
    public static readonly Color BrightCyan = Color.Parse("#80FFFF");
    public static readonly Color BrightYellow = Color.Parse("#FFFF80");
    public static readonly Color BrightWhite = Color.Parse("#C0C0C0");
    public static readonly Color DarkBlue = Color.Parse("#000080");
    public static readonly Color DarkRed = Color.Parse("#800000");
    public static readonly Color DarkMagenta = Color.Parse("#800080");
    public static readonly Color DarkGreen = Color.Parse("#008000");
    public static readonly Color DarkCyan = Color.Parse("#008080");
    public static readonly Color DarkYellow = Color.Parse("#808000");
    public static readonly Color DarkGrey = Color.Parse("#404040");
    public static readonly Color MidBlue = Color.Parse("#4040FF");
    public static readonly Color MidRed = Color.Parse("#FF4040");
    public static readonly Color MidGreen = Color.Parse("#40FF40");
    public static readonly Color MidCyan = Color.Parse("#40FFFF");

    // Optional: A method to get all colors as an array
    public static Color[] GetAllColors()
    {
        return new[]
        {
            Black, Blue, Red, Magenta, Green, Cyan, Yellow, White, Grey,
            BrightBlue, BrightRed, BrightMagenta, BrightGreen, BrightCyan, BrightYellow, BrightWhite,
            DarkBlue, DarkRed, DarkMagenta, DarkGreen, DarkCyan, DarkYellow, DarkGrey,
            MidBlue, MidRed, MidGreen, MidCyan
        };
    }
}


