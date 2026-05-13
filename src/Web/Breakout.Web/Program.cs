using Breakout;
using Breakout.Resources.Fonts;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

DrawnExtensions.RegisterFont("FontEmoji", "/fonts/NotoColorEmoji-Regular.ttf");

// Text / UI fonts
DrawnExtensions.RegisterFont("FontText", "/fonts/ZenMaruGothic-Bold.ttf");

// Game fonts
DrawnExtensions.RegisterFont("FontGame", "/fonts/DelaGothicOne-Regular.ttf");
DrawnExtensions.RegisterFont("FontGameKo", "/fonts/BlackHanSans-Regular.ttf");
DrawnExtensions.RegisterFont("FontGameZh", "/fonts/MaShanZheng-Regular.ttf");
DrawnExtensions.RegisterFont("FontSystem", "/fonts/amstrad_cpc464.ttf");

// Game images
DrawnExtensions.RegisterImage(@"Images\glass.jpg", "/Images/glass.jpg");
DrawnExtensions.RegisterImage(@"Images\back.jpg", "/Images/back.jpg");

var host = await builder.UseDrawnUiAsync(new DrawnUiStartupSettings
{
    UseDesktopKeyboard = true
});

AppLanguage.ApplySelected();

await host.RunAsync();
