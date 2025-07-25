﻿global using DrawnUi.Draw;
global using DrawnUi.Gaming;
global using Breakout.Game;
global using Breakout.Resources.Fonts;
global using Breakout.Resources.Strings;
using Microsoft.Extensions.Logging;
using Orbit.Input;
using Plugin.Maui.Audio;

namespace Breakout;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .AddAppFonts()
            .AddAudio() //Plugin.Maui.Audio
            .UseOrbitGameController(options => { }) //Game controller support
            .UseDrawnUi(new()
            {
                UseDesktopKeyboard = true,
                DesktopWindow = new()
                {
#if WINDOWS
                    Width = BreakoutGame.WIDTH + 15,
                    Height = BreakoutGame.HEIGHT + 40,
#else
                    Width = BreakoutGame.WIDTH,
                    Height = BreakoutGame.HEIGHT,
#endif
                }
            });

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