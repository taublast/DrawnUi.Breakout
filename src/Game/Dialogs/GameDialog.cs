namespace Breakout.Game.Dialogs
{
    /// <summary>
    /// Template system for customizing dialog appearance and behavior
    /// </summary>
    public class DialogTemplate
    {
        public Func<GameDialog, SkiaControl, string, string, SkiaLayout> CreateDialogFrame { get; set; }
        public Func<SkiaLayout> CreateBackdrop { get; set; }
        public Func<string, SkiaControl> CreateButton { get; set; }
        public DialogAnimations Animations { get; set; }
    }

    /// <summary>
    /// Animation definitions for dialog appearance and disappearance
    /// </summary>
    public class DialogAnimations
    {
        public Func<SkiaLayout, CancellationToken, Task> BackdropAppearing { get; set; }
        public Func<SkiaLayout, CancellationToken, Task> BackdropDisappearing { get; set; }
        public Func<SkiaLayout, CancellationToken, Task> FrameAppearing { get; set; }
        public Func<SkiaLayout, CancellationToken, Task> FrameDisappearing { get; set; }
    }

    /// <summary>
    /// A standalone dialog class with customizable templates and animations.
    /// Displays content with optional OK and Cancel buttons.
    /// </summary>
    public class GameDialog : SkiaLayout
    {
        // Navigation stack: container -> dialog
        private static readonly Dictionary<SkiaLayout, Stack<GameDialog>> _navigationStacks = new();

        // Template system for customizing dialog appearance
        public static DialogTemplate DefaultTemplate { get; set; }

        static GameDialog()
        {
            DefaultTemplate = DialogThemes.Modern;
        }

        // Legacy animation delegates (kept for backward compatibility)
        public static Func<SkiaLayout, SkiaLayout, SkiaLayout, CancellationToken, Task> DefaultAppearingAnimation
        {
            get;
            set;
        }

        public static Func<SkiaLayout, SkiaLayout, SkiaLayout, CancellationToken, Task> DefaultDisappearingAnimation
        {
            get;
            set;
        }


        public Action OnOkClicked { get; set; }
        public Action OnCancelClicked { get; set; }

        public SkiaControl Content;
        private string _okText;
        private string _cancelText;
        private SkiaLayout _parentContainer;
        private TaskCompletionSource<bool> _taskCompletionSource;
        private bool _isClosing;

        // References to dialog components for separate animations
        private SkiaLayout _dimmerLayer;
        private SkiaLayout _dialogFrame;

        private DialogTemplate _template;

        private GameDialog(SkiaControl content, string ok = null, string cancel = null,
            SkiaLayout parentContainer = null, DialogTemplate template = null)
        {
            Content = content;
            _okText = ok ?? ResStrings.BtnOk;
            _cancelText = cancel;
            _parentContainer = parentContainer;
            _template = template ?? DefaultTemplate;

            SetupDialog();
        }

        public override ISkiaGestureListener ProcessGestures(SkiaGesturesParameters args,
            GestureEventProcessingInfo apply)
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
        /// <param name="template">Custom template to use for this dialog (optional)</param>
        public static void Show(SkiaLayout parentContainer, SkiaControl content, string ok = null, string cancel = null,
            Action onOk = null, Action onCancel = null, DialogTemplate template = null)
        {
            var dialog = new GameDialog(content, ok, cancel, parentContainer, template);

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
        /// <param name="template">Custom template to use for this dialog (optional)</param>
        /// <returns>Task that returns true for OK, false for Cancel</returns>
        public static Task<bool> ShowAsync(SkiaLayout parentContainer, SkiaControl content, string ok = null,
            string cancel = null, DialogTemplate template = null)
        {
            var dialog = new GameDialog(content, ok, cancel, parentContainer, template);
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
        protected virtual async Task PlayAppearingAnimation(SkiaLayout parent, SkiaLayout dimmer, SkiaLayout frame,
            CancellationToken cancellationToken = default)
        {
            // Check template animations first
            if (_template?.Animations?.FrameAppearing != null && _template?.Animations?.BackdropAppearing != null)
            {
                var tasks = new List<Task>();

                if (dimmer != null && _template.Animations.BackdropAppearing != null)
                {
                    tasks.Add(_template.Animations.BackdropAppearing(dimmer, cancellationToken));
                }

                if (frame != null && _template.Animations.FrameAppearing != null)
                {
                    tasks.Add(_template.Animations.FrameAppearing(frame, cancellationToken));
                }

                await Task.WhenAll(tasks);
            }
            // Fallback to legacy animation system
            else if (DefaultAppearingAnimation != null)
            {
                await DefaultAppearingAnimation(parent, dimmer, frame, cancellationToken);
            }
            else
            {
                // Default appearing animation: dimmer fades in, frame scales up with fade
                var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                var tasks = new List<Task>();

                // Dimmer: Only fade in
                if (dimmer != null)
                {
                    dimmer.Opacity = 0.0;
                    tasks.Add(dimmer.FadeToAsync(1.0, 150, Easing.Linear, cancelSource));
                }

                foreach (var child in frame.Children)
                {
                    child.Scale = 0.8;
                    var frameScaleTask = child.ScaleToAsync(1.0, 1.0, 100, Easing.CubicOut, cancelSource);
                    tasks.Add(frameScaleTask);

                    if (child is not SkiaBackdrop)
                    {
                        child.Opacity = 0.0;
                        var frameFadeTask = child.FadeToAsync(1.0, 75, Easing.Linear, cancelSource);
                        tasks.Add(frameFadeTask);
                    }
                }

                // Frame: Scale up from center with fade in
                //frame.Scale = 0.8;
                //frame.Opacity = 0.0;
                //var frameScaleTask = frame.ScaleToAsync(1.0, 1.0, 5250, Easing.CubicOut, cancelSource);
                //var frameFadeTask = frame.FadeToAsync(1.0, 5200, Easing.Linear, cancelSource);

                await Task.WhenAll(tasks);
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
        protected virtual async Task PlayDisappearingAnimation(SkiaLayout parent, SkiaLayout dimmer, SkiaLayout frame,
            CancellationToken cancellationToken = default)
        {
            // Check template animations first
            if (_template?.Animations?.FrameDisappearing != null && _template?.Animations?.BackdropDisappearing != null)
            {
                var tasks = new List<Task>();

                if (dimmer != null && _template.Animations.BackdropDisappearing != null)
                {
                    tasks.Add(_template.Animations.BackdropDisappearing(dimmer, cancellationToken));
                }

                if (frame != null && _template.Animations.FrameDisappearing != null)
                {
                    tasks.Add(_template.Animations.FrameDisappearing(frame, cancellationToken));
                }

                await Task.WhenAll(tasks);
            }
            // Fallback to legacy animation system
            else if (DefaultDisappearingAnimation != null)
            {
                await DefaultDisappearingAnimation(parent, dimmer, frame, cancellationToken);
            }
            else
            {
                // Default disappearing animation: dimmer fades out, frame scales down with fade
                var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                var tasks = new List<Task>();


                // Dimmer: Only fade out
                if (dimmer != null)
                {
                    tasks.Add(dimmer.FadeToAsync(0.0, 150, Easing.Linear, cancelSource));
                }

                // Frame: Scale down with fade out
                tasks.Add(frame.ScaleToAsync(0.8, 0.8, 150, Easing.CubicIn, cancelSource));
                tasks.Add(frame.FadeToAsync(0.0, 150, Easing.Linear, cancelSource));

                await Task.WhenAll(tasks);
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
        public static void Push(SkiaLayout parentContainer, SkiaControl content, string ok = null, string cancel = null,
            Action onOk = null, Action onCancel = null, DialogTemplate template = null)
        {
            var dialog = new GameDialog(content, ok, cancel, parentContainer, template);

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
                var dialog = stack.Pop();
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

        protected void SetupDialog()
        {
            // Main dialog container
            HorizontalOptions = LayoutOptions.Fill;
            VerticalOptions = LayoutOptions.Fill;
            ZIndex = 200;

            // Always use template system (default is Game template which recreates original design)
            SetupDialogWithTemplate();
        }

        public static GameDialog GetTopDialog(SkiaLayout parentContainer)
        {
            if (!_navigationStacks.ContainsKey(parentContainer))
                return null;

            var stack = _navigationStacks[parentContainer];
            if (stack.Count == 0)
                return null;

            return stack.Last();
        }

        protected virtual void SetupDialogWithTemplate()
        {
            // Create backdrop if template provides one
            if (_template.CreateBackdrop != null)
            {
                _dimmerLayer = _template.CreateBackdrop();
            }

            // Create dialog frame using template - pass dialog instance for callbacks
            if (_template.CreateDialogFrame != null)
            {
                _dialogFrame = _template.CreateDialogFrame(this, Content, _okText, _cancelText);
            }
            else
            {
                throw new InvalidOperationException("Template must provide CreateDialogFrame");
            }

            // Add children
            var children = new List<SkiaControl>();
            if (_dimmerLayer != null)
            {
                children.Add(_dimmerLayer);
            }

            children.Add(_dialogFrame);

            Children = children;
        }

        /// <summary>
        /// Public method for templates to close dialog with OK result
        /// </summary>
        public async Task CloseWithOkAsync()
        {
            await CloseWithOkAsync(animate: true);
        }

        /// <summary>
        /// Public method for templates to close dialog with Cancel result
        /// </summary>
        public async Task CloseWithCancelAsync()
        {
            await CloseWithCancelAsync(animate: true);
        }

    }
}