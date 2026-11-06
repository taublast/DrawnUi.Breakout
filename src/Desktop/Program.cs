using System.Runtime.Versioning;
using Breakout.Game;
using DrawnUi;
using DrawnUi.Draw;
using DrawnUi.Gaming;
using DrawnUi.Infrastructure.Enums;
using DrawnUi.OpenTk;
using DrawnUi.Views;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

Super.UseDrawnUi()
    .ConfigureFonts(fonts =>
    {
        fonts.AddFont("fonts/ZenMaruGothic-Bold.ttf", AppFonts.Default);
        fonts.AddFont("fonts/DelaGothicOne-Regular.ttf", AppFonts.Game);
        fonts.AddFont("fonts/BlackHanSans-Regular.ttf", AppFonts.GameKo);
        fonts.AddFont("fonts/MaShanZheng-Regular.ttf", AppFonts.GameZh);
        fonts.AddFont("fonts/amstrad_cpc464.ttf", "FontSystem");
    })
    .Build();

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

SkiaImageManager.ReuseBitmaps = true;
AppLanguage.ApplySelected();

var backgroundImagePath = Path.Combine(AppContext.BaseDirectory, "Images", "back.jpg");

var gameSettings = new GameWindowSettings();

var nativeSettings = new NativeWindowSettings
{
    ClientSize = new Vector2i((int)(BreakoutGame.WIDTH * 1.33f), (int)(BreakoutGame.HEIGHT * 1.33f)),
    Title = "Bricks Breaker - DrawnUI OpenTK",
    API = ContextAPI.OpenGL,
    APIVersion = OperatingSystem.IsLinux() ? new Version(3, 3) : new Version(4, 6),
    Profile = ContextProfile.Core,
    WindowState = OpenTK.Windowing.Common.WindowState.Normal,
    Icon = LoadWindowIcon(),
};

// title-bar icon from the embedded icon.ico (ApplicationIcon covers only Explorer/taskbar)
OpenTK.Windowing.Common.Input.WindowIcon? LoadWindowIcon()
{
    try
    {
        var asm = System.Reflection.Assembly.GetExecutingAssembly();
        var name = asm.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("icon.ico", StringComparison.OrdinalIgnoreCase));
        if (name == null) return null;

        using var stream = asm.GetManifestResourceStream(name);
        if (stream == null) return null;

        using var bitmap = SKBitmap.Decode(stream);
        if (bitmap == null) return null;

        var resized = bitmap.Width != 32 || bitmap.Height != 32
            ? bitmap.Resize(new SKImageInfo(32, 32), SkiaSamplingOptions.GetSamplingOptions(FilterQuality.Ultra, true))
            : bitmap;

        var pixels = resized.Bytes;
        // SKBitmap is BGRA; OpenTK wants RGBA
        for (int i = 0; i < pixels.Length; i += 4)
            (pixels[i], pixels[i + 2]) = (pixels[i + 2], pixels[i]);

        var image = new OpenTK.Windowing.Common.Input.Image(32, 32, pixels);
        if (!ReferenceEquals(resized, bitmap)) resized.Dispose();
        return new OpenTK.Windowing.Common.Input.WindowIcon(image);
    }
    catch { return null; }
}

var canvas = new Breakout.RescalingCanvas
{
    LogicalWidth = BreakoutGame.WIDTH,
    LogicalHeight = BreakoutGame.HEIGHT,
    BackgroundColor = Colors.Black,
    RenderingMode = RenderingModeType.Accelerated,
    UpdateMode = UpdateModeType.Constant,
    HorizontalOptions = LayoutOptions.Fill,
    VerticalOptions = LayoutOptions.Fill,
    Content = new SkiaLayer
    {
        HorizontalOptions = LayoutOptions.Fill,
        VerticalOptions = LayoutOptions.Fill,
        Children =
        {
            new SkiaImage(backgroundImagePath)
            {
                UseCache = SkiaCacheType.GPU,
                AddEffect = SkiaImageEffect.Darken,
                Darken = 0.2,
            }.Fill(),

            new BreakoutGame
            {
                WidthRequest = BreakoutGame.WIDTH,
                HeightRequest = BreakoutGame.HEIGHT,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            },
#if DEBUG
            new SkiaLabelFps
            {
                UseCache = SkiaCacheType.GPU,
                Margin = new Thickness(0, 0, 4, 24),
                VerticalOptions = LayoutOptions.End,
                HorizontalOptions = LayoutOptions.End,
                Rotation = -45,
                FontSize = 11,
                BackgroundColor = Colors.DarkRed,
                TextColor = Colors.White,
                ZIndex = 110,
            }
#endif
        }
    }
};

using var window = new BreakoutDesktopWindow(gameSettings, nativeSettings, canvas);
window.Run();

internal sealed class BreakoutDesktopWindow(GameWindowSettings gs, NativeWindowSettings ns, Canvas canvas)
    : DrawnUiWindow(gs, ns, canvas)
{
    [SupportedOSPlatform("windows")]
    protected override void ConfigureWindowChrome(IntPtr hwnd)
    {
        WindowChrome.SetCaptionColor(hwnd, 0x21, 0x25, 0x29);
        WindowChrome.SetBorderColor(hwnd, 0x21, 0x25, 0x29);
    }

    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (OpenTkKeyMapper.Map(e.Key) is { } key)
        {
            KeyboardManager.KeyboardPressed(key);
        }
    }

    protected override void OnKeyUp(KeyboardKeyEventArgs e)
    {
        base.OnKeyUp(e);
        if (OpenTkKeyMapper.Map(e.Key) is { } key)
        {
            KeyboardManager.KeyboardReleased(key);
        }
    }
}