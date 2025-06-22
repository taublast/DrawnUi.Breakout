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
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("amstrad_cpc464.ttf", "FontGame");

                fonts.AddFont("DOM.TTF", "FontDrawn");
                fonts.AddFont("DOMB.TTF", "FontDrawnBold");

            });

        builder.UseDrawnUi(new()
            {
                UseDesktopKeyboard = true, 
                DesktopWindow = new()
                {
                    Width = 550,
                    Height = 750,
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