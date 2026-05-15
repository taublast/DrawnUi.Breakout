using System.Runtime.InteropServices.JavaScript;

namespace Breakout.Helpers;

public static class BrowserDeviceInfo
{
    public static bool IsMobileBrowser() => BrowserInterop.IsMobileBrowser();

    public static bool IsDesktopBrowser() => !IsMobileBrowser();
}

internal static partial class BrowserInterop
{
    [JSImport("globalThis.breakoutBrowser.isMobileBrowser")]
    internal static partial bool IsMobileBrowser();
}