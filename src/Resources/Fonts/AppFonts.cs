namespace Breakout.Resources.Fonts;

public static class AppFonts
{
    public const string Default = "FontText";
    public const string Game = "FontGame";

    public static MauiAppBuilder AddAppFonts(this MauiAppBuilder builder)
    {

        builder.ConfigureFonts(fonts =>
        {
            fonts.AddFont("ZenMaruGothic-Bold.ttf", AppFonts.Default);
            fonts.AddFont("DelaGothicOne-Regular.ttf", AppFonts.Game);
            fonts.AddFont("amstrad_cpc464.ttf", "FontSystem");
        });

        return builder;
    }

}