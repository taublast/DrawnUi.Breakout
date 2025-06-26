global using DrawnUi.Draw;
global using DrawnUi.Gaming;
using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;

namespace Breakout;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Chewy-Regular.ttf", "FontText");
                fonts.AddFont("amstrad_cpc464.ttf", "FontGame");
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

//#if WINDOWS
//            // game mode !!!
//            Thread.CurrentThread.Priority = ThreadPriority.Highest;
//#endif

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