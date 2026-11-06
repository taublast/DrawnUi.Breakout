# Bricks Breaker

A cross-platform Arkanoid/Breakout style game to play on **iOS, MacCatalyst, Android, Windows, Linux and in Browser**, made with DrawnUI for .NET.

### Install

* [Play in browser](https://taublast.github.io/DrawnUi.Breakout) 

* [Google Play](https://play.google.com/store/apps/details?id=com.appomobi.drawnui.breakout) 

* [AppStore](https://apps.apple.com/us/app/bricks-breaker/id6749823869?platform=iphone) 

* Run on Windows, Linux and MacCatalyst compiling from source code

## Game Features
* 12 levels of madness, ball versus bricks!
* Catch powerups destroying the bricks!
* If you are lucky enough shoot at bricks in Destroyer mode!
* Discover hidden music by catching rare powerups
* Auto-generated levels
* Available in 9 languages
* Play with touch/keyboard/mouse/apple controllers

---

Please star ⭐ if you like it!

Support: [ask your question here](https://github.com/taublast/DrawnUi.Breakout/discussions)

🤓 Read the article: [Breakout: Building A Cross-Platform Game in .NET MAUI with DrawnUI and Hot Preview](https://taublast.github.io/posts/Breakout)

https://github.com/user-attachments/assets/0bd6e340-a895-42e6-83f4-558f2323d37d


### What's New

* Desktop version for Windows and Linux
* Browser version loads much faster
* Ready for Android 16
* Physical keyboard and controller support

## How To Play

### With touch:

Tap the bottom-left corner button to open Settings.  
By default you can move one finger everywhere, the bottom HUD would be just the conviniet area to place it.  
Settings: if you enable "Press HUD" mode you would then need to press the left or right arrows of bottom HUD to move the paddle instead of panning the finger everywhere

### With keyboard/mouse/controller:

Left, Right to move Paddle  
Up, Down to move inside Settings  
Space/Enter, to Fire  
Esc to open Settings  

Discover game content on your own, no sploilers here!


## App Features
* Cross-platform for iOS, MacCatalyst, Android, Windows and Linux with hardware acceleration
* Control with touch/mouse/keyboard, customizable keys
* Background music and multichannel sounds
* Localized to 9 languages, auto-selects font upon language
* Auto-scales to any screen/window size
* AI-controlled paddle
* Auto-generated random levels
* Auto-unblock stuck ball
* Raycast collision detection
* Layered rendering
* Dialogs with glass-style transparent backdrops
* Input controllers support

## Development Features
* Compatible with .NET HotReload, built with Fluent C#
* All game field content is built with vector shapes and scalable to infinity
* One shared game project compiled into every head: .NET MAUI (iOS, MacCatalyst, Android, Windows), Blazor WebAssembly (browser) and OpenTK (Windows and Linux desktop)
* Windows: x64 can run both MSIX-packaged and "Steam-friendly" unpackaged mode, or as a single-file OpenTK exe with an installer
* Uses [Hot Preview](https://github.com/BretJohnson/hot-preview) to dynamically preview levels/states/languages/sprites and much more
* Uses [Soundflow](https://github.com/LSXPrime/SoundFlow) for sound on Android
* Uses [Plugin.Maui.Audio](https://github.com/jfversluis/Plugin.Maui.Audio) for sound on iOS, MacCatalyst and Windows (MAUI)
* Uses the browser Web Audio API for sound in the browser
* Uses [Miniaudio.Net](https://www.nuget.org/packages/Miniaudio.Net) for sound on desktop (OpenTK)
* Uses [OpenTK](https://github.com/opentk/opentk) for the desktop window and OpenGL on Windows and Linux
* Uses [DrawnUI for .NET](https://github.com/DrawnUi/DrawnUi.Net) for layout/gestures/fluent/bindings/rendering pipeline
* Uses [SkiaSharp](https://github.com/mono/SkiaSharp) to make this all possible

## Development Notes
* Actuallly you need to install Hot Preview dotnet tool to compile the app if previews nuget is referenced:
    ```bash
    dotnet tool install -g HotPreview.DevTools
    ```
* `MainPage.HotPreview.cs` contains all the HotPreview items. Read an article about [how to use HotPreview](https://github.com/BretJohnson/hot-preview). 
* `Soundflow` solved Android performance playing sounds/music.

### Desktop: Windows and Linux (OpenTK)

Desktop host project:

```text
src/Desktop/Breakout.Desktop.csproj
```

Run from source:

```bash
dotnet run --project ./src/Desktop/Breakout.Desktop.csproj --configuration Debug
```

Publish framework-dependent for Linux:

```bash
dotnet publish ./src/Desktop/Breakout.Desktop.csproj --configuration Release -r linux-x64 --self-contained false
```

Publish self-contained for Linux:

```bash
dotnet publish ./src/Desktop/Breakout.Desktop.csproj --configuration Release -r linux-x64 --self-contained true
```

Publish output:

```text
src/Desktop/bin/Release/net10.0/linux-x64/publish/
```

On Linux:

```bash
chmod +x ./BricksBreaker
./BricksBreaker
```

Linux runtime requirements:

* graphical desktop session
* working GPU or Mesa drivers
* OS OpenGL/window-system runtime libraries for your distro

Windows single-file exe (self-contained, trimmed):

```bash
dotnet publish ./src/Desktop/Breakout.Desktop.csproj -p:PublishProfile=FolderProfile
```

Output: `src/Desktop/bin/Release/net10.0/publish/win-x64/BricksBreaker.exe`. Compile `src/Desktop/innosetup.iss` with Inno Setup 6 to get the `BricksBreakerSetup.exe` installer.

### Blazor/Web

* Web versioning uses a hardcoded constant `AppConstants.Version` in `src/Shared/Helpers/AppConstants.cs`.
* On Blazor startup, `src/Web/App.razor` compares the current version with the last version stored in browser local storage via shared `AppSettings`.
* If versions mismatch, the app stores the new version and forces a hard browser reload with `NavigateTo(..., forceLoad: true)` so stale WASM/runtime state is dropped and fresh assets are loaded.
* When shipping a new web build, bump `AppConstants.Version`.
* Fonts are served as subsets that contain only the characters the game draws (`src/Web/wwwroot/fonts/*-Subset.ttf`, 0.6 MB instead of 35 MB). After changing localized strings or emoji, rebuild them with `python dev/fonts/subset_web_fonts.py` (needs `pip install fonttools`).

## Credits

* **Music** - All by `Eric Matyas` ([Soundimage.org](https://Soundimage.org)) please visit his site if you need high quality gaming audio content
* **Sound FX** - Those by `Eric Matyas` ([Soundimage.org](https://Soundimage.org)): powerup27, quirky7, quirky26, synthchime2, bells1
* **Glassy App Icons** - The settings button by `Laura Reen`, fell in love with the settings icon, licensed under CC Attribution
* **App Screenshots** - created with [Hotpot](https://hotpot.ai/)
 
### Optional Maybe ToDo

* Indestructibe bricks flash when hit
* Add enemies, main interest is they move and the ball is bouncing from them unexpectedly
* An AI-boss paddle would appear on top after all bricks destroyed would need to be outplayed
* Add shaders effects
* Add Pong mode!
