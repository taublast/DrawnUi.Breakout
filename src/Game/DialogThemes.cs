using DrawnUi.Draw;
using Microsoft.Maui.Graphics;

namespace BreakoutGame.Game
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
            
            CreateDialogFrame = (content, okText, cancelText) => new SkiaLayout
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
                        Children = CreateModernContentChildren(content, okText, cancelText)
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


        private static List<SkiaControl> CreateModernContentChildren(SkiaControl content, string okText, string cancelText)
        {
            var children = new List<SkiaControl>();

            if (content != null)
            {
                content.VerticalOptions = LayoutOptions.Start;
                children.Add(content);
            }

            children.Add(CreateModernButtonLayout(okText, cancelText));
            return children;
        }


        private static SkiaLayout CreateModernButtonLayout(string okText, string cancelText)
        {
            var layout = new SkiaLayout
            {
                Type = LayoutType.Row,
                HorizontalOptions = LayoutOptions.Center,
                Spacing = 16,
                Children = { CreateModernButton(okText, true) }
            };

            if (!string.IsNullOrEmpty(cancelText))
            {
                layout.Add(CreateModernButton(cancelText, false));
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
            
            CreateDialogFrame = (content, okText, cancelText) => new SkiaLayout
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
                        Children = CreateRetroContentChildren(content, okText, cancelText)
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

        private static List<SkiaControl> CreateRetroContentChildren(SkiaControl content, string okText, string cancelText)
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

            children.Add(CreateRetroButtonLayout(okText, cancelText));
            return children;
        }

        private static SkiaLayout CreateRetroButtonLayout(string okText, string cancelText)
        {
            var layout = new SkiaLayout
            {
                Type = LayoutType.Row,
                HorizontalOptions = LayoutOptions.Center,
                Spacing = 12,
                Children = { CreateRetroButton(okText, true) }
            };
            
            if (!string.IsNullOrEmpty(cancelText))
            {
                layout.Add(CreateRetroButton(cancelText, false));
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
    }
}
