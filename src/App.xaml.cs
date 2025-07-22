using AppoMobi.Specials;
using Breakout.Helpers;


namespace Breakout;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
#if PREVIEWS
        PreviewService.Initialize();
#endif
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new NavigationPage(new MainPage()));
    }

    protected override void OnStart()
    {
        base.OnStart();

        Tasks.StartDelayed(TimeSpan.FromSeconds(3),
            () => { Dispatcher.Dispatch(() => { DeviceDisplay.Current.KeepScreenOn = true; }); });
    }

    protected override void OnSleep()
    {
        base.OnSleep();

        Dispatcher.Dispatch(() => { DeviceDisplay.Current.KeepScreenOn = false; });

        Breakout.Game.BreakoutGame.Instance?.Pause();
    }

    protected override void OnResume()
    {
        base.OnResume();

        Dispatcher.Dispatch(() => { DeviceDisplay.Current.KeepScreenOn = true; });

        Breakout.Game.BreakoutGame.Instance?.Resume();
    }
}