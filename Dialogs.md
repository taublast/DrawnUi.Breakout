# GameDialog System Documentation

A dialog system for DrawnUI game with navigation stack support, customizable animations, and fluent API.

## Table of Contents

1. [Basic Usage](#basic-usage)
2. [Navigation Stack](#navigation-stack)
3. [API Reference](#api-reference)
4. [Examples](#examples)
5. [Best Practices](#best-practices)
6. [Enhanced Usage](#enhanced-usage)

## Basic Usage

### Simple Dialog (Fire-and-forget)

```csharp
var content = new SkiaLabel()
{
    Text = "Welcome to the game!",
    TextColor = Colors.White,
    FontSize = 16,
    HorizontalTextAlignment = DrawTextAlignment.Center,
    HorizontalOptions = LayoutOptions.Fill,
};

GameDialog.Show(this, content, "START", onOk: () =>
{
    // Handle OK button click - dialog auto-closes, no need to call close methods
    StartGame();
});
```

Just an info dialog with no result:

```csharp
GameDialog.Show(this, content, "Close");
```

Await before closing it:

```csharp
await GameDialog.ShowAsync(this, content, "Close");
```

### Dialog with OK and Cancel

```csharp
GameDialog.Show(this, content, "SAVE", "CANCEL", 
    onOk: () => SaveGame(),
    onCancel: () => CancelSave());
```

### Async Dialog (Wait for Result)

```csharp
bool result = await GameDialog.ShowAsync(this, content, "YES", "NO");

if (result)
{
    // User clicked YES
    DoSomething();
}
else
{
    // User clicked NO
    DoSomethingElse();
}
```

## Navigation Stack

Dialogs have a navigation stack that tracks dialogs per container.

### Key Concepts

- **Both `Show` and `Push` add to the stack**
- **Stack is per-container**: Each `SkiaLayout` has its own dialog stack
- **Automatic cleanup**: Empty stacks are removed automatically
- **LIFO behavior**: Last dialog shown is first to be popped
- **Auto-close on button click**: Dialogs automatically close when any button is clicked
- **Pop methods for programmatic closing**: Use `Pop`/`PopAll` when you need to close dialogs without button clicks

### Push Dialogs

```csharp
// Push first dialog
GameDialog.Push(this, content1, "NEXT", "CANCEL", 
    onOk: () => {
        // Push second dialog on top
        GameDialog.Push(this, content2, "FINISH", "BACK",
            onOk: () => GameDialog.PopAll(this),      // Close all
            onCancel: () => GameDialog.Pop(this));    // Go back one
    },
    onCancel: () => GameDialog.PopAll(this));
```

### Pop Operations

```csharp
// Pop topmost dialog (no result)
await GameDialog.Pop(container);

// Pop with animation control
await GameDialog.Pop(container, animate: false);

// Pop and wait for animation to finish
await GameDialog.PopAsync(container);

// Pop all dialogs (like "pop to root")
await GameDialog.PopAll(container);
await GameDialog.PopAllAsync(container);

// Check stack depth
int count = GameDialog.GetStackCount(container);

// Check if any dialog is currently open (visible)
bool isOpen = GameDialog.IsAnyDialogOpen(container);
```

## API Reference

### Static Methods

#### Show Methods
```csharp
// Basic show (adds to stack)
static void Show(SkiaLayout container, SkiaControl content, 
                string ok = null, string cancel = null, 
                Action onOk = null, Action onCancel = null)

// Async show (adds to stack, returns result)
static Task<bool> ShowAsync(SkiaLayout container, SkiaControl content, 
                           string ok = null, string cancel = null)
```

#### Navigation Stack Methods
```csharp
// Push dialog (same as Show, but explicit about stack behavior)
static void Push(SkiaLayout container, SkiaControl content, 
                string ok = null, string cancel = null, 
                Action onOk = null, Action onCancel = null)

// Pop operations
static Task Pop(SkiaLayout container, bool animate = true)
static Task PopAsync(SkiaLayout container, bool animate = true)
static Task PopAll(SkiaLayout container, bool animate = true)
static Task PopAllAsync(SkiaLayout container, bool animate = true)

// Stack info
static int GetStackCount(SkiaLayout container)
static bool IsAnyDialogOpen(SkiaLayout container)
```

### Instance Methods

```csharp
// Close with specific result
Task CloseAsync(bool result, bool animate = true)

// Convenience close methods
Task CloseWithOkAsync(bool animate = true)
Task CloseWithCancelAsync(bool animate = true)
```

## Examples

### Game Menu System

```csharp
void ShowMainMenu()
{
    var menuContent = new SkiaLabel()
    {
        Text = "Main Menu",
        TextColor = Colors.White,
        FontSize = 20,
        HorizontalTextAlignment = DrawTextAlignment.Center,
        HorizontalOptions = LayoutOptions.Fill,
    };

    GameDialog.Show(this, menuContent, "PLAY", "SETTINGS",
        onOk: () => StartGame(),
        onCancel: () => ShowSettings());
}

void ShowSettings()
{
    var settingsContent = new SkiaLabel()
    {
        Text = "Settings Menu",
        TextColor = Colors.White,
        FontSize = 16,
        HorizontalTextAlignment = DrawTextAlignment.Center,
        HorizontalOptions = LayoutOptions.Fill,
    };

    GameDialog.Push(this, settingsContent, "BACK", onOk: () =>
    {
        GameDialog.Pop(this); // Go back to main menu
    });
}
```

### Confirmation Dialog

```csharp
async Task<bool> ConfirmQuit()
{
    var content = new SkiaLabel()
    {
        Text = "Are you sure you want to quit?",
        TextColor = Colors.White,
        FontSize = 16,
        HorizontalTextAlignment = DrawTextAlignment.Center,
        HorizontalOptions = LayoutOptions.Fill,
    };

    return await GameDialog.ShowAsync(this, content, "YES", "NO");
}

// Usage
if (await ConfirmQuit())
{
    Application.Current.Quit();
}
```

### Multi-Step Wizard

```csharp
void ShowWizard()
{
    ShowStep1();
}

void ShowStep1()
{
    var content = CreateStepContent("Step 1 of 3", "Enter your name");
    
    GameDialog.Push(this, content, "NEXT", "CANCEL",
        onOk: () => ShowStep2(),
        onCancel: () => GameDialog.PopAll(this));
}

void ShowStep2()
{
    var content = CreateStepContent("Step 2 of 3", "Choose difficulty");
    
    GameDialog.Push(this, content, "NEXT", "BACK",
        onOk: () => ShowStep3(),
        onCancel: () => GameDialog.Pop(this));
}

void ShowStep3()
{
    var content = CreateStepContent("Step 3 of 3", "Confirm settings");
    
    GameDialog.Push(this, content, "FINISH", "BACK",
        onOk: () => {
            GameDialog.PopAll(this);
            StartGame();
        },
        onCancel: () => GameDialog.Pop(this));
}
```

### Auto-Close vs Programmatic Close

```csharp
// ✅ CORRECT: Dialog auto-closes when button is clicked
GameDialog.Show(this, content, "SAVE", "CANCEL",
    onOk: () => {
        SaveGame(); // Just handle the action - dialog closes automatically
    },
    onCancel: () => {
        CancelSave(); // Just handle the action - dialog closes automatically
    });

// ✅ CORRECT: Programmatic close (when no button was clicked)
void OnGameTimeout()
{
    // Close any open dialogs programmatically
    GameDialog.PopAll(this);
}

// ❌ WRONG: Don't call close methods in button callbacks
GameDialog.Show(this, content, "OK", onOk: () => {
    DoSomething();
    GameDialog.Pop(this); // ❌ Unnecessary - dialog already auto-closes!
});
```

## Best Practices

1. **Use `Show` for simple dialogs** that don't need stack management
2. **Use `Push` when building complex flows** that users might navigate back through
3. **Always handle both OK and Cancel** actions appropriately
4. **Use `PopAll` for "cancel everything"** scenarios
5. **Don't call close methods in button callbacks** - dialogs auto-close when buttons are clicked
6. **Use Pop methods only for programmatic closing** - when you need to close dialogs without user interaction
7. **Use `IsAnyDialogOpen()` to check if dialogs are visible** - more reliable than `GetStackCount()`

## Notes

- **Dialogs auto-close on button clicks**: No need to call close methods in your button callbacks
- **Pop methods are for programmatic closing**: Use when you need to close dialogs without button interaction
- Dialogs automatically remove themselves from the navigation stack when closed
- Empty navigation stacks are automatically cleaned up
- Both `Show` and `Push` add dialogs to the stack - they're functionally equivalent
- `Pop` and `PopAsync` don't return results - they just close dialogs
- Use `ShowAsync` when you need to wait for user input and get a boolean result
- **Default animations are included**: Scale up/down with fade in/out using DrawnUI methods
- **Cancellation token support**: All animations can be cancelled properly
- **DrawnUI animation methods**: Use `ScaleToAsync`, `FadeToAsync`, `TranslateToAsync` with millisecond durations
- **Separate dimmer and frame animations**: Dimmer (background) and frame (content) can be animated independently

---

## Enhanced Usage

*This section contains advanced features that most users won't need. The above sections cover all the essential functionality.*

### Animation Customization

By default, dialogs use DrawnUI animation methods with scale and fade effects. You can customize them globally or per-dialog if needed.

#### DrawnUI Animation Methods

DrawnUI provides its own animation methods that differ from MAUI animations:

- **`ScaleToAsync(x, y, duration, easing, cancellationTokenSource)`** - Scale animation
- **`FadeToAsync(opacity, duration, easing, cancellationTokenSource)`** - Fade animation
- **`TranslateToAsync(x, y, duration, easing, cancellationTokenSource)`** - Translation animation
- **`AnimateRangeAsync(callback, start, end, duration, easing, cancellationTokenSource)`** - Custom range animation

Key points:
- Duration is in **milliseconds**
- Uses **`CancellationTokenSource`** (not `CancellationToken`)
- Supports **automatic cancellation** of previous animations

#### Separate Dimmer and Frame Animations

Dialogs consist of two main visual components:
- **Dimmer**: The background overlay that dims the content behind the dialog
- **Frame**: The actual dialog content (buttons, text, etc.)

These components can be animated separately:
- **Dimmer typically**: Only fades in/out for smooth background transitions
- **Frame can do**: Complex animations like scale, slide, bounce, etc.

This separation allows for more sophisticated and visually appealing dialog animations.

### Global Animation Customization

```csharp
// Set global appearing animation with separate dimmer and frame animations
GameDialog.DefaultAppearingAnimation = async (parent, dimmer, frame, cancellationToken) =>
{
    var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

    // Dimmer: Only fade in (background overlay)
    dimmer.Opacity = 0.0;
    var dimmerTask = dimmer.FadeToAsync(1.0, 200, Easing.Linear, cancelSource);

    // Frame: Scale up with fade in (dialog content)
    frame.Scale = 0.5;
    frame.Opacity = 0.0;
    var frameScaleTask = frame.ScaleToAsync(1.0, 1.0, 250, Easing.CubicOut, cancelSource);
    var frameFadeTask = frame.FadeToAsync(1.0, 200, Easing.Linear, cancelSource);

    await Task.WhenAll(dimmerTask, frameScaleTask, frameFadeTask);
};

// Set global disappearing animation with separate dimmer and frame animations
GameDialog.DefaultDisappearingAnimation = async (parent, dimmer, frame, cancellationToken) =>
{
    var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

    // Dimmer: Only fade out
    var dimmerTask = dimmer.FadeToAsync(0.0, 150, Easing.Linear, cancelSource);

    // Frame: Scale down with fade out
    var frameScaleTask = frame.ScaleToAsync(0.8, 0.8, 150, Easing.CubicIn, cancelSource);
    var frameFadeTask = frame.FadeToAsync(0.0, 150, Easing.Linear, cancelSource);

    await Task.WhenAll(dimmerTask, frameScaleTask, frameFadeTask);
};
```

### Custom Dialog with Override

```csharp
public class SlideDialog : GameDialog
{
    protected override async Task PlayAppearingAnimation(SkiaLayout parent, SkiaLayout dimmer, SkiaLayout frame, CancellationToken cancellationToken = default)
    {
        var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Dimmer: Quick fade in
        dimmer.Opacity = 0;
        var dimmerTask = dimmer.FadeToAsync(1.0, 150, Easing.Linear, cancelSource);

        // Frame: Slide in from top with bounce
        frame.TranslationY = -200;
        frame.Opacity = 0;
        var frameSlideTask = frame.TranslateToAsync(0, 0, 400, Easing.BounceOut, cancelSource);
        var frameFadeTask = frame.FadeToAsync(1.0, 300, Easing.Linear, cancelSource);

        await Task.WhenAll(dimmerTask, frameSlideTask, frameFadeTask);
    }

    protected override async Task PlayDisappearingAnimation(SkiaLayout parent, SkiaLayout dimmer, SkiaLayout frame, CancellationToken cancellationToken = default)
    {
        var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Dimmer: Fade out
        var dimmerTask = dimmer.FadeToAsync(0.0, 150, Easing.Linear, cancelSource);

        // Frame: Slide out to bottom
        var frameSlideTask = frame.TranslateToAsync(0, 200, 200, Easing.CubicIn, cancelSource);
        var frameFadeTask = frame.FadeToAsync(0.0, 200, Easing.Linear, cancelSource);

        await Task.WhenAll(dimmerTask, frameSlideTask, frameFadeTask);
    }
}
```

### Animation Properties (Advanced)

```csharp
// Global animation delegates with separate dimmer and frame control
static Func<SkiaLayout, SkiaLayout, SkiaLayout, CancellationToken, Task> DefaultAppearingAnimation { get; set; }
static Func<SkiaLayout, SkiaLayout, SkiaLayout, CancellationToken, Task> DefaultDisappearingAnimation { get; set; }
```

### Virtual Methods (Override in Subclasses)

```csharp
protected virtual Task PlayAppearingAnimation(SkiaLayout parent, SkiaLayout dimmer, SkiaLayout frame, CancellationToken cancellationToken = default)
protected virtual Task PlayDisappearingAnimation(SkiaLayout parent, SkiaLayout dimmer, SkiaLayout frame, CancellationToken cancellationToken = default)
```
