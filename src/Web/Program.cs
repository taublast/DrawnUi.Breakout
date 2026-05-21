using Breakout;
using Breakout.Resources.Fonts;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

var assetBaseUri = new Uri(builder.HostEnvironment.BaseAddress);

DrawnExtensions.RegisterFont("FontEmoji", BuildAssetUrl(assetBaseUri, "fonts/NotoColorEmoji-Regular.ttf"));

// Text / UI fonts
DrawnExtensions.RegisterFont("FontText", BuildAssetUrl(assetBaseUri, "fonts/ZenMaruGothic-Bold.ttf"));

// Game fonts
DrawnExtensions.RegisterFont("FontGame", BuildAssetUrl(assetBaseUri, "fonts/DelaGothicOne-Regular.ttf"));
DrawnExtensions.RegisterFont("FontGameKo", BuildAssetUrl(assetBaseUri, "fonts/BlackHanSans-Regular.ttf"));
DrawnExtensions.RegisterFont("FontGameZh", BuildAssetUrl(assetBaseUri, "fonts/MaShanZheng-Regular.ttf"));
DrawnExtensions.RegisterFont("FontSystem", BuildAssetUrl(assetBaseUri, "fonts/amstrad_cpc464.ttf"));

// Game images
DrawnExtensions.RegisterImage("Images/glass.jpg", BuildAssetUrl(assetBaseUri, "Images/glass.jpg"));
DrawnExtensions.RegisterImage("Images/back.jpg", BuildAssetUrl(assetBaseUri, "Images/back.jpg"));

var host = await builder.UseDrawnUiAsync(new DrawnUiStartupSettings
{
    UseDesktopKeyboard = true
});

AppLanguage.ApplySelected();

await host.RunAsync();

static string BuildAssetUrl(Uri baseUri, string relativePath)
{
    return new Uri(baseUri, relativePath).ToString();
}
