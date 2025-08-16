using DrawnUi.Maui.Controls;
using DrawnUi.Maui.Draw;

namespace FruityBob;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        Content = new SkiaShell()
        {
            BackgroundColor = Colors.DarkGreen,
            Children =
            {
                new SkiaLabel()
                {
                    Text = "🍎 FruityBob 🍊",
                    FontSize = 48,
                    TextColor = Colors.Yellow,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontAttributes = FontAttributes.Bold
                },
                new SkiaLabel()
                {
                    Text = "Inspired by Fruity Frank on Amstrad CPC 6128",
                    FontSize = 16,
                    TextColor = Colors.LightGreen,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 100, 0, 0)
                }
            }
        };
    }
}
