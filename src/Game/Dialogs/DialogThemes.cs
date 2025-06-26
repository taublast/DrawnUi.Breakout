using DrawnUi.Draw;
using Microsoft.Maui.Graphics;

namespace BreakoutGame.Game.Dialogs
{
    /// <summary>
    /// Predefined dialog themes for easy customization
    /// </summary>
    public static class DialogThemes
    {
        #region MODERN

        /// <summary>
        /// Modern glass-like dialog theme with blur effects
        /// </summary>
        public static DialogTemplate Modern => new DialogTemplate
        {
            CreateBackdrop = () => new SkiaLayout
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.Black.WithAlpha(0.5f)
            },
            
            CreateDialogFrame = (dialog, content, okText, cancelText) => new SkiaLayout
            {
                Margin = 40,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                MinimumHeightRequest = 50,
                Children = new List<SkiaControl>
                {
                    // Modern backdrop with blur
                    new SkiaShape
                    {
                        CornerRadius = 16,
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Fill,
                        BackgroundColor = Colors.White.WithAlpha(0.9f),
                        //Shadow = new SkiaShadow 
                        //{ 
                        //    Color = Colors.Black.WithAlpha(0.3f), 
                        //    Blur = 20,
                        //    OffsetY = 8
                        //}
                    },
                    
                    // Content container
                    new SkiaLayout
                    {
                        Type = LayoutType.Column,
                        Padding = 32,
                        Spacing = 24,
                        HorizontalOptions = LayoutOptions.Fill,
                        Children = CreateModernContentChildren(dialog, content, okText, cancelText)
                    }
                }
            },
            
            CreateButton = (text) => new SkiaButton
            {
                Text = text,
                FontSize = 16,
                TextColor = Colors.White,
                BackgroundColor = Colors.Blue,
                CornerRadius = 8,
                Padding = new Thickness(24, 12),
                MinimumWidthRequest = 100
            },
            
            Animations = new DialogAnimations
            {
                BackdropAppearing = async (backdrop, token) =>
                {
                    var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(token);
                    backdrop.Opacity = 0;
                    await backdrop.FadeToAsync(1.0, 300, Easing.Linear, cancelSource);
                },

                FrameAppearing = async (frame, token) =>
                {
                    var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(token);
                    frame.Scale = 0.8;
                    frame.Opacity = 0;
                    await Task.WhenAll(
                        frame.ScaleToAsync(1.0, 1.0, 400, Easing.CubicOut, cancelSource),
                        frame.FadeToAsync(1.0, 300, Easing.Linear, cancelSource)
                    );
                },

                BackdropDisappearing = async (backdrop, token) =>
                {
                    var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(token);
                    await backdrop.FadeToAsync(0.0, 200, Easing.Linear, cancelSource);
                },

                FrameDisappearing = async (frame, token) =>
                {
                    var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(token);
                    await Task.WhenAll(
                        frame.ScaleToAsync(0.9, 0.9, 200, Easing.CubicIn, cancelSource),
                        frame.FadeToAsync(0.0, 200, Easing.Linear, cancelSource)
                    );
                }
            }
        };


        private static List<SkiaControl> CreateModernContentChildren(GameDialog dialog, SkiaControl content, string okText, string cancelText)
        {
            var children = new List<SkiaControl>();

            if (content != null)
            {
                content.VerticalOptions = LayoutOptions.Start;
                children.Add(content);
            }

            children.Add(CreateModernButtonLayout(dialog, okText, cancelText));
            return children;
        }


        private static SkiaLayout CreateModernButtonLayout(GameDialog dialog, string okText, string cancelText)
        {
            var okButton = CreateModernButton(okText, true);
            okButton.OnTapped(async (me) => await dialog.CloseWithOkAsync());

            var layout = new SkiaLayout
            {
                Type = LayoutType.Row,
                HorizontalOptions = LayoutOptions.Center,
                Spacing = 16,
                Children = { okButton }
            };

            if (!string.IsNullOrEmpty(cancelText))
            {
                var cancelButton = CreateModernButton(cancelText, false);
                cancelButton.OnTapped(async (me) => await dialog.CloseWithCancelAsync());
                layout.Add(cancelButton);
            }

            return layout;
        }

        private static SkiaButton CreateModernButton(string text, bool isPrimary)
        {
            return new SkiaButton
            {
                Text = text,
                FontSize = 16,
                TextColor = Colors.White,
                BackgroundColor = isPrimary ? Colors.Blue : Colors.Gray,
                CornerRadius = 8,
                Padding = new Thickness(24, 12),
                MinimumWidthRequest = 100
            };
        }



        #endregion

        #region RETRO

        /// <summary>
        /// Retro terminal-style dialog theme
        /// </summary>
        public static DialogTemplate Retro => new DialogTemplate
        {
            CreateBackdrop = () => new SkiaLayout
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.Black.WithAlpha(0.8f)
            },
            
            CreateDialogFrame = (dialog, content, okText, cancelText) => new SkiaLayout
            {
                Margin = 60,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Children = new List<SkiaControl>
                {
                    // Retro terminal background
                    new SkiaShape
                    {
                        CornerRadius = 0,
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Fill,
                        BackgroundColor = Colors.Black,
                        StrokeColor = Colors.LimeGreen,
                        StrokeWidth = 2
                    },
                    
                    // Content container
                    new SkiaLayout
                    {
                        Type = LayoutType.Column,
                        Padding = 24,
                        Spacing = 16,
                        HorizontalOptions = LayoutOptions.Fill,
                        Children = CreateRetroContentChildren(dialog, content, okText, cancelText)
                    }
                }
            },
            
            CreateButton = (text) => new SkiaButton
            {
                Text = text,
                FontSize = 14,
                FontFamily = "FontGame",
                TextColor = Colors.LimeGreen,
                BackgroundColor = Colors.Black,
                StrokeColor = Colors.LimeGreen,
                StrokeWidth = 1,
                Padding = new Thickness(16, 8),
                MinimumWidthRequest = 80
            },
            
            Animations = new DialogAnimations
            {
                BackdropAppearing = async (backdrop, token) =>
                {
                    var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(token);
                    backdrop.Opacity = 0;
                    await backdrop.FadeToAsync(1.0, 150, Easing.Linear, cancelSource);
                },

                FrameAppearing = async (frame, token) =>
                {
                    var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(token);
                    frame.Opacity = 0;
                    await frame.FadeToAsync(1.0, 200, Easing.Linear, cancelSource);
                },

                BackdropDisappearing = async (backdrop, token) =>
                {
                    var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(token);
                    await backdrop.FadeToAsync(0.0, 100, Easing.Linear, cancelSource);
                },

                FrameDisappearing = async (frame, token) =>
                {
                    var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(token);
                    await frame.FadeToAsync(0.0, 150, Easing.Linear, cancelSource);
                }
            }
        };

        private static List<SkiaControl> CreateRetroContentChildren(GameDialog dialog, SkiaControl content, string okText, string cancelText)
        {
            var children = new List<SkiaControl>();

            if (content != null)
            {
                content.VerticalOptions = LayoutOptions.Start;
                // Style content for retro theme
                if (content is SkiaLabel label)
                {
                    label.TextColor = Colors.LimeGreen;
                    label.FontFamily = "FontGame";
                }
                children.Add(content);
            }

            children.Add(CreateRetroButtonLayout(dialog, okText, cancelText));
            return children;
        }

        private static SkiaLayout CreateRetroButtonLayout(GameDialog dialog, string okText, string cancelText)
        {
            var okButton = CreateRetroButton(okText, true);
            okButton.OnTapped(async (me) => await dialog.CloseWithOkAsync());

            var layout = new SkiaLayout
            {
                Type = LayoutType.Row,
                HorizontalOptions = LayoutOptions.Center,
                Spacing = 12,
                Children = { okButton }
            };

            if (!string.IsNullOrEmpty(cancelText))
            {
                var cancelButton = CreateRetroButton(cancelText, false);
                cancelButton.OnTapped(async (me) => await dialog.CloseWithCancelAsync());
                layout.Add(cancelButton);
            }

            return layout;
        }

        private static SkiaButton CreateRetroButton(string text, bool isPrimary)
        {
            return new SkiaButton
            {
                Text = text,
                FontSize = 14,
                FontFamily = "FontGame",
                TextColor = Colors.LimeGreen,
                BackgroundColor = Colors.Black,
                StrokeColor = Colors.LimeGreen,
                StrokeWidth = 1,
                Padding = new Thickness(16, 8),
                MinimumWidthRequest = 80
            };
        }

        #endregion

        #region GAME

        private static float gameRadius = 8;
        /// <summary>
        /// Game-specific dialog theme (recreates the original design exactly)
        /// </summary>
        public static DialogTemplate Game => new DialogTemplate
        {
            CreateBackdrop = () => null, // Original design doesn't use dimmer layer


            CreateDialogFrame = (dialog, content, okText, cancelText) => new SkiaLayout
            {
                Margin = 32,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                MinimumHeightRequest = 80,
                Children = new List<SkiaControl>
                {
                    //shape A - background texture for frosted effect plus shadow (cached layer)
                    new SkiaShape()
                    {
                        UseCache = SkiaCacheType.Image,
                        BackgroundColor = Color.Parse("#10ffffff"),
                        CornerRadius = gameRadius,
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Fill,
                        StrokeColor = Colors.Red,
                        StrokeWidth = 2,
                        StrokeGradient = new SkiaGradient()
                        {
                            Opacity = 0.99f,
                            StartXRatio = 0.2f,
                            EndXRatio = 0.5f,
                            StartYRatio = 0.0f,
                            EndYRatio = 1f,
                            Colors = new Color[]
                            {
                                Color.Parse("#ffffff"),
                                Color.Parse("#999999"),
                            }
                        },
                        Children =
                        {
                            new SkiaImage()
                            {
                                Opacity = 0.25,
                                Source = "Images/glass.jpg",
                                HorizontalOptions = LayoutOptions.Fill,
                                VerticalOptions = LayoutOptions.Fill,
                            }
                        }
                    },

                    //shape B = backdrop
                    new SkiaShape()
                    {
                        CornerRadius = gameRadius,
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Fill,
                        Children =
                        {
                            new SkiaBackdrop()
                            {
                                Blur = 4,
                                HorizontalOptions = LayoutOptions.Fill,
                                VerticalOptions = LayoutOptions.Fill,
                            }
                        }
                    },

                    // Content layout
                    new SkiaLayout()
                    {
                        ZIndex = 1,
                        HorizontalOptions = LayoutOptions.Fill,
                        UseCache = SkiaCacheType.Image,
                        Type = LayoutType.Column,
                        Padding = 20,
                        Spacing = 28,
                        Children = CreateGameContentChildren(dialog, content, okText, cancelText)
                    }
                }
            }
        };

        private static List<SkiaControl> CreateGameContentChildren(GameDialog dialog, SkiaControl content, string okText, string cancelText)
        {
            var children = new List<SkiaControl>();

            // Add the main content
            if (content != null)
            {
                content.VerticalOptions = LayoutOptions.Start;
                children.Add(content);
            }

            // Create buttons layout exactly like the original
            var buttonsLayout = new SkiaLayout()
            {
                Type = LayoutType.Row,
                Margin = new(0, 8, 0, 8),
                HorizontalOptions = LayoutOptions.Center,
                Spacing = 16,
                Children =
                {
                    // OK button using UiElements.Button (original design) with proper callback
                    BreakoutGame.UiElements.Button(okText, async () => await dialog.CloseWithOkAsync())
                }
            };

            // Cancel button (optional) - exactly like original
            if (!string.IsNullOrEmpty(cancelText))
            {
                var cancelButton = new SkiaButton()
                {
                    Text = cancelText,
                    FontSize = 14,
                    FontFamily = "FontGame",
                    TextColor = Colors.White,
                    BackgroundColor = Colors.DarkRed,
                    WidthRequest = -1,
                    MinimumWidthRequest = 100,
                };

                cancelButton.OnTapped(async (me) => await dialog.CloseWithCancelAsync());
                buttonsLayout.Add(cancelButton);
            }

            children.Add(buttonsLayout);
            return children;
        }

        #endregion
    }
}
