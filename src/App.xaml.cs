using AppoMobi.Specials;

namespace Breakout;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage());
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

        BreakoutGame.Game.BreakoutGame.Instance?.Pause();
    }

    protected override void OnResume()
    {
        base.OnResume();

        Dispatcher.Dispatch(() => { DeviceDisplay.Current.KeepScreenOn = true; });

        BreakoutGame.Game.BreakoutGame.Instance?.Resume();
    }
}