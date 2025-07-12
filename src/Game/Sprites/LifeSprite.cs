namespace Breakout.Game;

public class LifeSprite : SkiaShape
{
    public LifeSprite()
    {
        Margin = new Thickness(1,1,3,3);
        WidthRequest = 26;
        HeightRequest = 8;
        StrokeColor = Color.Parse("#CCCCFF");
        StrokeWidth = -1;
        CornerRadius = 4;
        BackgroundColor = Colors.DarkOrange;
        Children = new List<SkiaControl>()
        {
            new SkiaShape()
            {
                CornerRadius = 2,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                BackgroundColor = AmstradColors.BrightGreen,
                Margin = new(4, 1),
                BevelType = BevelType.Emboss,
                Bevel = new SkiaBevel()
                {
                    Depth = 2,
                    LightColor = Colors.White,
                    ShadowColor = Color.Parse("#333333"),
                    Opacity = 0.33,
                }
            }
        };
        //Shadows = new List<SkiaShadow>()
        //{
        //    new SkiaShadow()
        //    {
        //        X = 2, Y = 2, Color = Colors.DarkBlue, Blur = 0
        //    }
        //} ;
    }
}