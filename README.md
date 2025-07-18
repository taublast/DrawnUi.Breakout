# Breakout

A cross-platform game to play on **iOS, MacCatalyst, Android and Windows**.

## Game Features
* 12 levels of Breakout madness!
* Fight paddle bosses after destroying the bricks!

## App Features
* Cross-platform for iOS, MacCatalyst, Android and Windows with hardware acceleration
* Control with touch/mouse/keyboard, customizable keys
* Localized to 9 languages
* Auto-scales to any screen/window size
* Background music and multichannel sounds
* Auto-generated random levels
* AI-controlled paddle
* Raycast collision detection
* In-game dialogs with glass-style dynamic transparent backdrops

## Development Features
* Windows: x64 runs both MSIX-packaged and "Steam-friendly" unpackaged mode
* Compatible with .NET HotReload, built with Fluent C#
* All game field content is built with vector shapes and scalable to infinity
* Uses [HotPreview]() to dynamically preview levels/states/languages/sprites and much more
* Uses [Plugin.Maui.Audio]() for sound
* Uses [DrawnUI for .NET MAUI]() for gestures/layout/bindings/fluent/rendering and more

## Development Notes
* `MainPage.HotPreview.cs` contains all the HotPreview items. Read an article about [how to use HotPreview](). 

## CREDITS

* **Glassy App Icons** by `Laura Reen` licenced under CC Attribution License
* todo music



## TODO

### Required!

* an AI-boss paddle will appear on top after all bricks destroyed
* make paddle start at center for real user after demo player..
* add on screen hud controls for mobile, remove for desktop
* fix geometry of some levels

### Optional Maybe

* add to settings dialog change language
* add to settings dialog music on/off
* add to settings dialog customize buttons
* add power-ups
* add enemies
* add enemies

### Very optional

* add Pong mode 

