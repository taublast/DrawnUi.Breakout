using Breakout.Game;
using DrawnUi.Views;

namespace DrawnUi.Draw
{
#if PREVIEWS

    using Microsoft.Maui.ApplicationModel;
    using PreviewFramework;
    using PreviewFramework.App.Maui;
    using PreviewFramework.SharedModel;
    using PreviewFramework.SharedModel.App;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Customizable MauiPreviewNavigatorService
    /// </summary>
    public class PreviewService : MauiPreviewNavigatorService
    {
        /// <summary>
        /// One-line for initialization
        /// </summary>
        public static void Initialize()
        {
            MauiPreviewApplication.Instance.PreviewNavigatorService = new PreviewService();
        }

        /// <summary>
        /// Can be customized
        /// </summary>
        public static Func<INavigation> NavigateFrom = () => { return Application.Current!.MainPage!.Navigation; };

        public override Task NavigateToPreviewAsync(UIComponentReflection uiComponent, PreviewReflection preview)
        {
            return MainThread.InvokeOnMainThreadAsync(async () =>
            {
                object? previewUI = preview.Create();

                if (uiComponent.Kind == UIComponentKind.Control)
                {
                    ContentPage controlsPage = new ContentPage
                    {
                        Content = (View)previewUI
                    };

                    await NavigateFrom().PushAsync(controlsPage, NavigateAnimationsEnabled);
                }
                else
                {
                    if (previewUI is RoutePreview routePreview)
                    {
                        Window? mainWindow = Application.Current!.Windows[0];
                        Shell? shell = mainWindow?.Page as Shell;


                        if (shell is null)
                        {
                            throw new InvalidOperationException("Main window doesn't use Shell");
                        }

                        await shell.GoToAsync(routePreview.Route, NavigateAnimationsEnabled);
                    }
                    else if (previewUI is ContentPage contentPage)
                    {
                        await NavigateFrom().PushAsync(contentPage, NavigateAnimationsEnabled);
                    }
                }
            });
        }


    }

#endif
}