global using DrawnUi.Draw;
global using DrawnUi.Gaming;
global using Breakout.Game;
global using BreakoutGame.Resources.Strings;
using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;

namespace Breakout;

public static class MauiProgram
{
    /// <summary>
    /// Enabled languages
    /// </summary>
    public static IEnumerable<string> Languages = new List<string>
    {
        "en", //FIRST is fallback
        "de",
        "es",
        "fr",
        "it",
        "ru",
        "ja",
        "ko",
        "zh",
    };

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("ZenMaruGothic-Bold.ttf", AppFonts.Default);
                fonts.AddFont("DelaGothicOne-Regular.ttf", AppFonts.Game);
                //fonts.AddFont("GothicA1-ExtraBold.ttf", AppFonts.Default);
                fonts.AddFont("amstrad_cpc464.ttf", "FontSystem");
            });

        builder.UseDrawnUi(new()
            {
                UseDesktopKeyboard = true,
                DesktopWindow = new()
                {
                    Width = 375,
                    Height = 800,
                    IsFixedSize = true
                    //todo disable maximize btn 
                }
            })
            .AddAudio();

        //to avoid returning many copies of same sprite bitmap for different images
        SkiaImageManager.ReuseBitmaps = true;

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    public static bool IsDebug
    {
        get
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}