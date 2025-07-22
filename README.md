# Breakout

A cross-platform game to play on **iOS, MacCatalyst, Android and Windows**.

## Game Features
* 12 levels of Breakout madness!
* Catch powerups destroying the bricks!
* If you are lucky enough shoot at bricks in Destroyer mode!
* Auto-generated levels
* Available in 9 languages

## App Features
* Cross-platform for iOS, MacCatalyst, Android and Windows with hardware acceleration
* Control with touch/mouse/keyboard, customizable keys
* Background music and multichannel sounds
* Localized to 9 languages, auto-selects font upon language
* Auto-scales to any screen/window size
* Auto-generated random levels
* AI-controlled paddle
* Raycast collision detection
* Layered rendering
* Dialogs with glass-style transparent backdrops

## Development Features
* Compatible with .NET HotReload, built with Fluent C#
* All game field content is built with vector shapes and scalable to infinity
* Windows: x64 can run both MSIX-packaged and "Steam-friendly" unpackaged mode
* Uses [HotPreview](https://github.com/BretJohnson/hot-preview) to dynamically preview levels/states/languages/sprites and much more
* Uses [Soundflow](https://github.com/LSXPrime/SoundFlow) and [Plugin.Maui.Audio](https://github.com/jfversluis/Plugin.Maui.Audio) for sound
* Uses [DrawnUI for .NET MAUI]() for layout/gestures/fluent/bindings/rendering pipeline
* Uses [SkiaSharp](https://github.com/mono/SkiaSharp) to make this all possible

## Development Notes
* `MainPage.HotPreview.cs` contains all the HotPreview items. Read an article about [how to use HotPreview](https://github.com/BretJohnson/hot-preview). 
* We use `Soundflow` on mobile and `Plugin.Maui.Audio` on Windows.
* Couln't use Soundflow on Windows as its background music code prohibits app from closing clicking on Close button 
and app is crashing with "Activating a single-threaded class from MTA is not supported" when calling `Dispatcher.Dispatch(() => { DeviceDisplay.Current.KeepScreenOn = true; });`.

## Credits

* **Music** - All by `Eric Matyas` ([Soundimage.org](https://Soundimage.org)) please visit his site if you need high quality gaming audio content
* **Sound FX** - Those by `Eric Matyas` ([Soundimage.org](https://Soundimage.org)): powerup27, quirky7, quirky26, synthchime2, bells1
* **Glassy App Icons** - The settings button by `Laura Reen`, fell in love with the settings icon, licensed under CC Attribution


### Optional Maybe ToDo

* add enemies, main interest is they move and the ball is bouncing from them unexpectedly
* an AI-boss paddle will appear on top after all bricks destroyed would need to be outplayed
* add Pong mode to be chooses along with usual breakout at start
