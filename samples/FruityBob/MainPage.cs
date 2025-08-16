using DrawnUi.Maui.Controls;
using DrawnUi.Maui.Draw;

namespace FruityBob;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        Content = new SkiaShell()
        {
            BackgroundColor = Colors.Black,
            Children =
            {
                new SkiaLabel()
                {
                    Text = "FruityBob - Coming Soon!",
                    FontSize = 32,
                    TextColor = Colors.Yellow,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            }
        };
    }
}
