using AppoMobi.Specials;

namespace Breakout.Game;

public class DisplayFlag : SkiaLayout
{
    private string _lang;

    public string Lang
    {
        get => _lang;
        set
        {
            if (value == _lang) return;
            _lang = value;
            OnPropertyChanged();
        }
    }

    public DisplayFlag()
    {
        HeightRequest = 28;
        WidthRequest = 56;
        Children = new List<SkiaControl>()
        {
            new SkiaLayout()
            {
                Type = LayoutType.Row,
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill,
                Spacing = 0,

                Children = new List<SkiaControl>()
                {
                    //flag icon
                    new SkiaShape()
                    {
                        Margin = new(0, 0, 2, 0),
                        StrokeColor = BreakoutGame.UiElements.ColorIconSecondary,
                        StrokeWidth = 1,
                        VerticalOptions = LayoutOptions.Fill,
                        HorizontalOptions = LayoutOptions.Fill,
                        Children =
                        {
                            new SkiaSvg()
                                {
                                    VerticalOptions = LayoutOptions.Fill,
                                    HorizontalOptions = LayoutOptions.Fill,
                                    Aspect = TransformAspect.Fill
                                }
                                .ObserveProperty(this, nameof(Lang), me =>
                                {
                                    if (!string.IsNullOrEmpty(this.Lang))
                                    {
                                        var resKey = $"SvgFlag{this.Lang.ToTitleCase()}";
                                        me.SvgString = App.Current.Resources.Get<string>(resKey);
                                    }
                                }),
                        }
                    },


                    //dropdown icon
                    new SkiaSvg()
                    {
                        Margin = new Microsoft.Maui.Thickness(1, 1, 0, 0),
                        HorizontalOptions = LayoutOptions.Start,
                        TintColor = BreakoutGame.UiElements.ColorIconSecondary,
                        VerticalOptions = LayoutOptions.Fill,
                        WidthRequest = 10,
                        SvgString = App.Current.Resources.Get<string>("SvgDropdown")
                    }
                },
            }
        };
    }
}