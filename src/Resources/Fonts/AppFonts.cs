namespace Breakout.Resources.Fonts;

public static class AppFonts
{
    /// <summary>
    /// Dialogs, default text
    /// </summary>
    public const string Default = "FontText";

    /// <summary>
    /// Score, buttons, game-style
    /// </summary>
    public const string Game = "FontGame";
    public const string GameKo = "FontGameKo";
    public const string GameZh = "FontGameZh";

    public static string GameAutoselect
    {
        get
        {
            if (!string.IsNullOrEmpty(_useGameFont))
            {
                return _useGameFont;
            }
            return Game;
        }
    }

    public static double GameAdjustSize
    {
        get
        {
            if (_adjustGameFont<=0)
            {
                return 1;
            }
            return _adjustGameFont;
        }
    }

    private static double _adjustGameFont;
    private static string _useGameFont;
    public static void UseGameFont(string value, double scale = 1.0)
    {
        _useGameFont = value;
        _adjustGameFont=scale;
    }

    public static MauiAppBuilder AddAppFonts(this MauiAppBuilder builder)
    {

        builder.ConfigureFonts(fonts =>
        {
            fonts.AddFont("ZenMaruGothic-Bold.ttf", AppFonts.Default); //dialogs
            fonts.AddFont("DelaGothicOne-Regular.ttf", AppFonts.Game); //score, buttons
            fonts.AddFont("BlackHanSans-Regular.ttf", AppFonts.GameKo); 
            fonts.AddFont("MaShanZheng-Regular.ttf", AppFonts.GameZh); 
            fonts.AddFont("amstrad_cpc464.ttf", "FontSystem");
        });

        return builder;
    }

}