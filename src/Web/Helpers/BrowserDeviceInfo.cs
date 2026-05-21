using DrawnUi.Views;

namespace Breakout.Helpers;

public static class BrowserDeviceInfo
{
    public static bool IsMobileBrowser() => BrowserApi.IsMobileBrowser();

    public static bool IsDesktopBrowser() => !IsMobileBrowser();
}