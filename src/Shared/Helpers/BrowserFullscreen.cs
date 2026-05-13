#if BROWSER
using System.Runtime.InteropServices.JavaScript;

namespace Breakout.Helpers;

public static class BrowserFullscreen
{
    public static bool IsEnabled()
    {
        try
        {
            return BrowserFullscreenInterop.IsEnabled();
        }
        catch
        {
            return false;
        }
    }

    public static void SetEnabled(bool enabled)
    {
        try
        {
            BrowserFullscreenInterop.SetEnabled(enabled);
        }
        catch
        {
            // Ignore unsupported browser fullscreen failures.
        }
    }
}

internal static partial class BrowserFullscreenInterop
{
    [JSImport("globalThis.breakoutFullscreen.isEnabled")]
    internal static partial bool IsEnabled();

    [JSImport("globalThis.breakoutFullscreen.setEnabled")]
    internal static partial void SetEnabled(bool enabled);
}
#endif