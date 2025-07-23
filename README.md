# Breakout

A cross-platform game to play on **iOS, MacCatalyst, Android and Windows**.

## Game Features
* 12 levels of Breakout madness!
* Catch powerups destroying the bricks!
* If you are lucky enough shoot at bricks in Destroyer mode!
* Discover hidden music by catching rare powerups
* Auto-generated levels
* Available in 9 languages
* Play with touch/keyboard/mouse/apple controllers

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
* Input controllers support

## Development Features
* Compatible with .NET HotReload, built with Fluent C#
* All game field content is built with vector shapes and scalable to infinity
* Windows: x64 can run both MSIX-packaged and "Steam-friendly" unpackaged mode
* Uses [Hot Preview](https://github.com/BretJohnson/hot-preview) to dynamically preview levels/states/languages/sprites and much more
* Uses [Soundflow](https://github.com/LSXPrime/SoundFlow) for sound on Android
* Uses [Plugin.Maui.Audio](https://github.com/jfversluis/Plugin.Maui.Audio) for sound on all other platforms
* Uses [DrawnUI for .NET MAUI](https://github.com/taublast/DrawnUi) for layout/gestures/fluent/bindings/rendering pipeline
* Uses [SkiaSharp](https://github.com/mono/SkiaSharp) to make this all possible

## Development Notes
* `MainPage.HotPreview.cs` contains all the HotPreview items. Read an article about [how to use HotPreview](https://github.com/BretJohnson/hot-preview). 
* `Soundflow` solved Android performance playing sounds/music.

## Credits

* **Music** - All by `Eric Matyas` ([Soundimage.org](https://Soundimage.org)) please visit his site if you need high quality gaming audio content
* **Sound FX** - Those by `Eric Matyas` ([Soundimage.org](https://Soundimage.org)): powerup27, quirky7, quirky26, synthchime2, bells1
* **Glassy App Icons** - The settings button by `Laura Reen`, fell in love with the settings icon, licensed under CC Attribution

### Optional Maybe ToDo

* New powerups: multiball, fireball
* Indestructibe bricks flash when hit
* Add enemies, main interest is they move and the ball is bouncing from them unexpectedly
* An AI-boss paddle would appear on top after all bricks destroyed would need to be outplayed
* Add shaders effects
* Add Pong mode!
