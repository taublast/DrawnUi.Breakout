# Breakout

A cross-platform game to play on **iOS, MacCatalyst, Android and Windows**.

## Game Features
* 12 levels of Breakout madness!
* Fight paddle bosses after destroying the bricks!

## App Features
* Cross-platform for iOS, MacCatalyst, Android and Windows with hardware acceleration
* Control with touch/mouse/keyboard, customizable keys
* Localized to 9 languages
* Select font upon language
* Auto-scales to any screen/window size
* Background music and multichannel sounds
* Auto-generated random levels
* AI-controlled paddle
* Raycast collision detection
* Layered rendering
* In-game dialogs with glass-style dynamic transparent backdrops

## Development Features
* Windows: x64 runs both MSIX-packaged and "Steam-friendly" unpackaged mode
* Compatible with .NET HotReload, built with Fluent C#
* All game field content is built with vector shapes and scalable to infinity
* Uses [HotPreview](https://github.com/BretJohnson/hot-preview) to dynamically preview levels/states/languages/sprites and much more
* Uses [Soundflow](https://github.com/LSXPrime/SoundFlow) and [Plugin.Maui.Audio](https://github.com/jfversluis/Plugin.Maui.Audio) for sound
* Uses [DrawnUI for .NET MAUI]() for gestures/layout/bindings/fluent/rendering and more

## Development Notes
* `MainPage.HotPreview.cs` contains all the HotPreview items. Read an article about [how to use HotPreview](https://github.com/BretJohnson/hot-preview). 

* We use `Soundflow` on mobile and Plugin.Maui.Audio` on Windows.
* Couln't use Soundflow on Windows as
it prohibited app from closing when clicking on Close button 
and was crashing with "Activating a single-threaded class from MTA is not supported" when calling `Dispatcher.Dispatch(() => { DeviceDisplay.Current.KeepScreenOn = true; });`.

## CREDITS

* **Glassy App Icons** by `Laura Reen` licenced under CC Attribution License
* todo music

### Optional Maybe ToDo

* add enemies
* an AI-boss paddle will appear on top after all bricks destroyed
* add Pong mode 