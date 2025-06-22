using DrawnUi.Draw;

namespace Breakout;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        try
        {
            InitializeComponent();
        }
        catch (Exception e)
        {
            Super.DisplayException(this, e);
        }
    } 
}

