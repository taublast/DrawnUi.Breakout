using DrawnUi.Draw;
using Microsoft.Maui.Graphics;

namespace BreakoutGame.Game
{
    /// <summary>
    /// A standalone dialog class with blurred background for the Breakout game.
    /// Displays content with optional OK and Cancel buttons.
    /// </summary>
    public class GameDialog : SkiaLayout
    {
        // Navigation stack: container -> dialog
        private static readonly Dictionary<SkiaLayout, Stack<GameDialog>> _navigationStacks = new();

        // Customizable animation delegates with cancellation token support
        public static Func<SkiaLayout, SkiaLayout, SkiaLayout, CancellationToken, Task> DefaultAppearingAnimation { get; set; }
        public static Func<SkiaLayout, SkiaLayout, SkiaLayout, CancellationToken, Task> DefaultDisappearingAnimation { get; set; }

        public Action OnOkClicked { get; set; }
        public Action OnCancelClicked { get; set; }

        private SkiaControl _content;
        private string _okText;
        private string _cancelText;
        private SkiaLayout _parentContainer;
        private TaskCompletionSource<bool> _taskCompletionSource;
        private bool _isClosing;

        // References to dialog components for separate animations
        private SkiaLayout _dimmerLayer;
        private SkiaLayout _dialogFrame;

        private GameDialog(SkiaControl content, string ok = null, string cancel = null, SkiaLayout parentContainer = null)
        {
            _content = content;
            _okText = ok ?? "OK";
            _cancelText = cancel;
            _parentContainer = parentContainer;

            SetupDialog();
        }

        public override ISkiaGestureListener ProcessGestures(SkiaGesturesParameters args, GestureEventProcessingInfo apply)
        {
            return base.ProcessGestures(args, apply);
        }

        /// <summary>
        /// Shows a dialog with the specified content and buttons.
        /// </summary>
        /// <param name="parentContainer">The parent container to add the dialog to</param>
        /// <param name="content">The content to display in the dialog</param>
        /// <param name="ok">OK button text (defaults to "OK")</param>
        /// <param name="cancel">Cancel button text (null = no cancel button)</param>
        /// <param name="onOk">Action to execute when OK is clicked</param>
        /// <param name="onCancel">Action to execute when Cancel is clicked</param>
        public static void Show(SkiaLayout parentContainer, SkiaControl content, string ok = null, string cancel = null, Action onOk = null, Action onCancel = null)
        {
            var dialog = new GameDialog(content, ok, cancel, parentContainer);

            // Add to navigation stack
            if (!_navigationStacks.ContainsKey(parentContainer))
            {
                _navigationStacks[parentContainer] = new Stack<GameDialog>();
            }
            _navigationStacks[parentContainer].Push(dialog);

            dialog.OnOkClicked = onOk;
            dialog.OnCancelClicked = onCancel;

            parentContainer.AddSubView(dialog);

            // Play appearing animation
            _ = dialog.PlayAppearingAnimation();
        }

        /// <summary>
        /// Shows a dialog asynchronously and returns true if OK was clicked, false if Cancel was clicked.
        /// </summary>
        /// <param name="parentContainer">The parent container to add the dialog to</param>
        /// <param name="content">The content to display in the dialog</param>
        /// <param name="ok">OK button text (defaults to "OK")</param>
        /// <param name="cancel">Cancel button text (null = no cancel button)</param>
        /// <returns>Task that returns true for OK, false for Cancel</returns>
        public static Task<bool> ShowAsync(SkiaLayout parentContainer, SkiaControl content, string ok = null, string cancel = null)
        {
            var dialog = new GameDialog(content, ok, cancel, parentContainer);
            dialog._taskCompletionSource = new TaskCompletionSource<bool>();

            // Add to navigation stack
            if (!_navigationStacks.ContainsKey(parentContainer))
            {
                _navigationStacks[parentContainer] = new Stack<GameDialog>();
            }
            _navigationStacks[parentContainer].Push(dialog);

            // Note: OnOkClicked and OnCancelClicked will be handled by the CloseAsync method
            // The task completion will be set there

            parentContainer.AddSubView(dialog);

            // Play appearing animation
            _ = dialog.PlayAppearingAnimation();

            return dialog._taskCompletionSource.Task;
        }

        /// <summary>
        /// Closes the dialog with the specified result and optional animation.
        /// </summary>
        /// <param name="result">The result to return (true for OK, false for Cancel)</param>
        /// <param name="animate">Whether to animate the closing</param>
        public async Task CloseAsync(bool result, bool animate = true)
        {
            if (_isClosing) return;
            _isClosing = true;

            if (animate)
            {
                await PlayDisappearingAnimation();
            }

            // Remove from parent
            _parentContainer?.RemoveSubView(this);

            // Remove from navigation stack
            if (_parentContainer != null && _navigationStacks.ContainsKey(_parentContainer))
            {
                var stack = _navigationStacks[_parentContainer];
                if (stack.Count > 0 && stack.Peek() == this)
                {
                    stack.Pop();
                }

                // Clean up empty stacks
                if (stack.Count == 0)
                {
                    _navigationStacks.Remove(_parentContainer);
                }
            }

            // Complete the task if it exists
            _taskCompletionSource?.SetResult(result);

            // Call the appropriate callback AFTER cleanup (user doesn't need to close dialog)
            if (result)
            {
                System.Diagnostics.Debug.WriteLine($"GameDialog: Calling OnOkClicked callback");
                OnOkClicked?.Invoke();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"GameDialog: Calling OnCancelClicked callback");
                OnCancelClicked?.Invoke();
            }
        }

        /// <summary>
        /// Closes the dialog with OK result.
        /// </summary>
        public Task CloseWithOkAsync(bool animate = true) => CloseAsync(true, animate);

        /// <summary>
        /// Closes the dialog with Cancel result.
        /// </summary>
        public Task CloseWithCancelAsync(bool animate = true) => CloseAsync(false, animate);

        /// <summary>
        /// Plays the appearing animation when the dialog is shown.
        /// Override this method to customize the appearing animation.
        /// </summary>
        /// <param name="parent">The parent container</param>
        /// <param name="dimmer">The dimmer/background layer</param>
        /// <param name="frame">The dialog frame/content</param>
        /// <param name="cancellationToken">Cancellation token for the animation</param>
        protected virtual async Task PlayAppearingAnimation(SkiaLayout parent, SkiaLayout dimmer, SkiaLayout frame, CancellationToken cancellationToken = default)
        {
            if (DefaultAppearingAnimation != null)
            {
                await DefaultAppearingAnimation(parent, dimmer, frame, cancellationToken);
            }
            else
            {
                // Default appearing animation: dimmer fades in, frame scales up with fade
                var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                // Dimmer: Only fade in
                dimmer.Opacity = 0;
                var dimmerTask = dimmer.FadeToAsync(1, 200, Easing.Linear, cancelSource);

                // Frame: Scale up from 0 with fade in
                frame.Scale = 0;
                frame.Opacity = 0;
                var frameScaleTask = frame.ScaleToAsync(1, 1, 250, Easing.CubicOut, cancelSource);
                var frameeFadeTask = frame.FadeToAsync(1, 200, Easing.Linear, cancelSource);

                await Task.WhenAll(dimmerTask, frameScaleTask, frameeFadeTask);
            }
        }

        /// <summary>
        /// Plays the disappearing animation when the dialog is closed.
        /// Override this method to customize the disappearing animation.
        /// </summary>
        /// <param name="parent">The parent container</param>
        /// <param name="dimmer">The dimmer/background layer</param>
        /// <param name="frame">The dialog frame/content</param>
        /// <param name="cancellationToken">Cancellation token for the animation</param>
        protected virtual async Task PlayDisappearingAnimation(SkiaLayout parent, SkiaLayout dimmer, SkiaLayout frame, CancellationToken cancellationToken = default)
        {
            if (DefaultDisappearingAnimation != null)
            {
                await DefaultDisappearingAnimation(parent, dimmer, frame, cancellationToken);
            }
            else
            {
                // Default disappearing animation: dimmer fades out, frame scales down with fade
                var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                // Dimmer: Only fade out
                var dimmerTask = dimmer.FadeToAsync(0, 150, Easing.Linear, cancelSource);

                // Frame: Scale down to 0.8 with fade out
                var frameScaleTask = frame.ScaleToAsync(0.8, 0.8, 150, Easing.CubicIn, cancelSource);
                var frameFadeTask = frame.FadeToAsync(0, 150, Easing.Linear, cancelSource);

                await Task.WhenAll(dimmerTask, frameScaleTask, frameFadeTask);
            }
        }

        /// <summary>
        /// Convenience method that calls PlayAppearingAnimation with proper parameters.
        /// </summary>
        private async Task PlayAppearingAnimation(CancellationToken cancellationToken = default)
        {
            await PlayAppearingAnimation(_parentContainer, _dimmerLayer, _dialogFrame, cancellationToken);
        }

        /// <summary>
        /// Convenience method that calls PlayDisappearingAnimation with proper parameters.
        /// </summary>
        private async Task PlayDisappearingAnimation(CancellationToken cancellationToken = default)
        {
            await PlayDisappearingAnimation(_parentContainer, _dimmerLayer, _dialogFrame, cancellationToken);
        }

        #region Navigation Stack Methods

        /// <summary>
        /// Pushes a dialog onto the navigation stack (equivalent to Show but adds to stack).
        /// </summary>
        public static void Push(SkiaLayout parentContainer, SkiaControl content, string ok = null, string cancel = null, Action onOk = null, Action onCancel = null)
        {
            var dialog = new GameDialog(content, ok, cancel, parentContainer);

            // Add to navigation stack
            if (!_navigationStacks.ContainsKey(parentContainer))
            {
                _navigationStacks[parentContainer] = new Stack<GameDialog>();
            }
            _navigationStacks[parentContainer].Push(dialog);

            dialog.OnOkClicked = onOk;
            dialog.OnCancelClicked = onCancel;

            parentContainer.AddSubView(dialog);

            // Play appearing animation
            _ = dialog.PlayAppearingAnimation();
        }

        /// <summary>
        /// Pops the topmost dialog from the navigation stack.
        /// </summary>
        /// <param name="parentContainer">The parent container</param>
        /// <param name="animate">Whether to animate the closing</param>
        public static async Task Pop(SkiaLayout parentContainer, bool animate = true)
        {
            if (!_navigationStacks.ContainsKey(parentContainer) || _navigationStacks[parentContainer].Count == 0)
                return;

            var dialog = _navigationStacks[parentContainer].Peek();
            await dialog.CloseAsync(false, animate); // No result, just close
        }

        /// <summary>
        /// Pops the topmost dialog asynchronously (waits for animation to finish).
        /// </summary>
        /// <param name="parentContainer">The parent container</param>
        /// <param name="animate">Whether to animate the closing</param>
        /// <returns>Task that completes when the dialog is closed</returns>
        public static async Task PopAsync(SkiaLayout parentContainer, bool animate = true)
        {
            await Pop(parentContainer, animate);
        }

        /// <summary>
        /// Pops all dialogs from the navigation stack.
        /// </summary>
        /// <param name="parentContainer">The parent container</param>
        /// <param name="animate">Whether to animate the closing</param>
        public static async Task PopAll(SkiaLayout parentContainer, bool animate = true)
        {
            if (!_navigationStacks.ContainsKey(parentContainer))
                return;

            var stack = _navigationStacks[parentContainer];
            var tasks = new List<Task>();

            while (stack.Count > 0)
            {
                var dialog = stack.Peek();
                tasks.Add(dialog.CloseAsync(false, animate));
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Pops all dialogs from the navigation stack and returns when all are closed.
        /// </summary>
        /// <param name="parentContainer">The parent container</param>
        /// <param name="animate">Whether to animate the closing</param>
        public static async Task PopAllAsync(SkiaLayout parentContainer, bool animate = true)
        {
            await PopAll(parentContainer, animate);
        }

        /// <summary>
        /// Gets the number of dialogs in the navigation stack for the specified container.
        /// </summary>
        /// <param name="parentContainer">The parent container</param>
        /// <returns>Number of dialogs in the stack</returns>
        public static int GetStackCount(SkiaLayout parentContainer)
        {
            return _navigationStacks.ContainsKey(parentContainer) ? _navigationStacks[parentContainer].Count : 0;
        }

        /// <summary>
        /// Checks if any dialog is currently open (visible) for the specified container.
        /// </summary>
        /// <param name="parentContainer">The parent container</param>
        /// <returns>True if any dialog is currently open, false otherwise</returns>
        public static bool IsAnyDialogOpen(SkiaLayout parentContainer)
        {
            if (!_navigationStacks.ContainsKey(parentContainer))
                return false;

            var stack = _navigationStacks[parentContainer];
            if (stack.Count == 0)
                return false;

            return true;
        }

        #endregion

        protected virtual void SetupDialog()
        {
            // Main dialog container
            HorizontalOptions = LayoutOptions.Fill;
            VerticalOptions = LayoutOptions.Fill;
            ZIndex = 200;

            // Create dimmer layer (background overlay)
            _dimmerLayer = new SkiaLayout()
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                BackgroundColor = Color.Parse("#66000000"),
                ZIndex = -1,
                UseCache = SkiaCacheType.Operations
            };

            // Create dialog frame (the actual dialog content)
            _dialogFrame = new SkiaLayout()
            {
                Margin = 50,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                MinimumHeightRequest = 50,
                //WidthRequest = 330,
                Children = new List<SkiaControl>()
                {
                    // Background shape with rounded corners
                    new SkiaShape()
                    {
                        CornerRadius = 14,
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Fill,
                        StrokeColor = Colors.White,
                        StrokeWidth = 2,
                        ZIndex = -1,
                        Children =
                        {
                         new SkiaBackdrop()
                            {
                                Blur = 6,
                                HorizontalOptions = LayoutOptions.Fill,
                                VerticalOptions = LayoutOptions.Fill,
                            }
                        }
                    },

                    // Content layout
                    new SkiaLayout()
                    {
                        HorizontalOptions = LayoutOptions.Fill, //todo required for some reason here, check why and fix
                        UseCache = SkiaCacheType.Image,
                        Type = LayoutType.Column,
                        Padding = 24,
                        Spacing = 20,
                        Children = CreateContentChildren()
                    }
                }
            };

            Children = new List<SkiaControl>()
            {
                _dimmerLayer,
                _dialogFrame
            };
        }

        protected virtual List<SkiaControl> CreateContentChildren()
        {
            var children = new List<SkiaControl>();

            // Add the main content
            if (_content != null)
            {
                _content.VerticalOptions = LayoutOptions.Start;
                children.Add(_content);
            }

            // Create buttons layout
            var buttonsLayout = new SkiaLayout()
            {
                Type = LayoutType.Row,
                HorizontalOptions = LayoutOptions.Center,
                Spacing = 16,
                Children =
                {
                    // OK button (always present)
                    new SkiaButton()
                    {
                        Text = _okText,
                        FontSize = 16,
                        FontFamily = "FontGame",
                        TextColor = Colors.White,
                        BackgroundColor = Colors.DarkBlue,
                        //CornerRadius = 8,
                        //Padding = new Thickness(24, 12),
                        WidthRequest = -1,
                        MinimumWidthRequest = 100,
                    }
                    .OnTapped(async (me) =>
                    {
                        System.Diagnostics.Debug.WriteLine($"GameDialog: OK button tapped, auto-closing dialog");
                        // Auto-close dialog - user doesn't need to call close methods in their callbacks
                        await CloseWithOkAsync();
                    })
                }
            };

            // Cancel button (optional)
            if (!string.IsNullOrEmpty(_cancelText))
            {
                var cancelButton = new SkiaButton()
                {
                    Text = _cancelText,
                    FontSize = 16,
                    FontFamily = "FontGame",
                    TextColor = Colors.White,
                    BackgroundColor = Colors.DarkRed,
                    //CornerRadius = 8,
                    //Padding = new Thickness(24, 12),
                    WidthRequest = -1,
                    MinimumWidthRequest = 100,
                }
                .OnTapped(async (me) =>
                {
                    System.Diagnostics.Debug.WriteLine($"GameDialog: Cancel button tapped, auto-closing dialog");
                    // Auto-close dialog - user doesn't need to call close methods in their callbacks
                    await CloseWithCancelAsync();
                });

                buttonsLayout.Add(cancelButton);
            }

            children.Add(buttonsLayout);

            return children;
        }
    }
}
