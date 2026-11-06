using Breakout;
using Breakout.Resources.Fonts;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

AppLanguage.ApplySelected();

await Super.UseDrawnUi(builder)
    .WithBaseUrl(builder.HostEnvironment.BaseAddress)
    .WithOptions(options => options.UseDesktopKeyboard = true)
    .ConfigureFonts(fonts =>
    {
        // Subsets with only the glyphs the game draws, built by dev/fonts/subset_web_fonts.py
        fonts.AddFont("fonts/NotoColorEmoji-Subset.ttf", "FontEmoji");

        // Text / UI font
        fonts.AddFont("fonts/ZenMaruGothic-Bold-Subset.ttf", AppFonts.Default);

        // Game fonts
        fonts.AddFont("fonts/DelaGothicOne-Regular-Subset.ttf", AppFonts.Game);
        fonts.AddFont("fonts/BlackHanSans-Regular-Subset.ttf", AppFonts.GameKo);
        fonts.AddFont("fonts/MaShanZheng-Regular-Subset.ttf", AppFonts.GameZh);
        fonts.AddFont("fonts/amstrad_cpc464.ttf", "FontSystem");
    })
    .ConfigureStyles(styles =>
    {
        // browsers have no system fonts: glyphs the UI font lacks (simplified hanzi, hangul) come from these
        styles.AddStyle(new Style()
        {
            ApplyToDerivedTypes = true,
            TargetType = typeof(SkiaLabel),
            Setters =
            {
                new Setter()
                {
                    Property = SkiaLabel.FontFamilyFallbackProperty,
                    Value = $"{AppFonts.GameZh}, {AppFonts.GameKo}, FontEmoji"
                }
            }
        });
    })
    .PreloadAssets(assets =>
    {
        assets.AddImage("Images/glass.jpg", "Images/glass.jpg");
        assets.AddImage("Images/back.jpg", "Images/back.jpg");
    })
    .BuildAndRunAsync();
