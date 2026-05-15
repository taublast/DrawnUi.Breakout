using System.Diagnostics;

namespace Breakout.Game
{
    public partial class BreakoutGame
    {
        private sealed record StartupAssetFailure(string Title, string Message, string? Details = null);

        private sealed record StartupAssetBatch(string StatusText, int TotalItems,
            Func<Action<int, int, string>, Task> LoadAsync);

        private Task? _startupAssetsTask;
        private bool _startupPresentationQueued;

        public bool ShowStartupGameContent
        {
            get;
            private set
            {
                if (value == field) return;
                field = value;
                OnPropertyChanged();
            }
        } = true;

        public bool ShowStartupAssetsOverlay { get; private set; }
        public string StartupAssetsStatusText { get; private set; } = ResStrings.LoadingAssets;
        public int StartupAssetsLoadedCount { get; private set; }
        public int StartupAssetsTotalCount { get; private set; }
        public float StartupAssetsProgress => StartupAssetsTotalCount <= 0
            ? 0f
            : (float)StartupAssetsLoadedCount / StartupAssetsTotalCount;

        private void BeginStartupAssetLoading()
        {
            SetStartupGameContentVisible(false);
            _startupAssetsTask = LoadStartupAssetsAsync();
        }

        private List<StartupAssetBatch> CreateStartupAssetBatches()
        {
            var batches = new List<StartupAssetBatch>();

            if (USE_SOUND)
            {
                batches.Add(new StartupAssetBatch(ResStrings.LoadingAssets, GetAudioStartupAssetCount(),
                    reportProgress => InitializeAudioAsync(reportProgress)));
            }

            return batches;
        }

        private async Task LoadStartupAssetsAsync()
        {
            var batches = CreateStartupAssetBatches();
            if (batches.Count == 0)
            {
                return;
            }

            int totalItems = 0;
            foreach (var batch in batches)
            {
                totalItems += batch.TotalItems;
            }

            int completedItems = 0;
            UpdateStartupAssetsProgress(0, totalItems, ResStrings.LoadingAssets);

            foreach (var batch in batches)
            {
                await batch.LoadAsync((loadedInBatch, _, statusText) =>
                {
                    UpdateStartupAssetsProgress(completedItems + loadedInBatch, totalItems, statusText);
                });

                completedItems += batch.TotalItems;
                UpdateStartupAssetsProgress(completedItems, totalItems, batch.StatusText);
            }
        }

        private void PrepareStartupPresentation()
        {
            var startupAssetsTask = _startupAssetsTask;
            if (startupAssetsTask == null || startupAssetsTask.IsCompleted)
            {
                PresentGame();
                return;
            }

            if (_startupPresentationQueued)
            {
                return;
            }

            _startupPresentationQueued = true;
            _ = AwaitStartupAssetsAndPresentAsync(startupAssetsTask);
        }

        private async Task AwaitStartupAssetsAndPresentAsync(Task startupAssetsTask)
        {
            StartupAssetFailure? startupFailure = null;

            try
            {
                var overlayDelayTask = Task.Delay(250);
                if (await Task.WhenAny(startupAssetsTask, overlayDelayTask) == overlayDelayTask &&
                    !startupAssetsTask.IsCompleted)
                {
                    SetStartupAssetsOverlayVisible(true);
                }

                await startupAssetsTask;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Startup asset loading failed: {ex}");
                startupFailure = CreateStartupAssetFailure(ex);
            }
            finally
            {
                SetStartupAssetsOverlayVisible(false);
                SetStartupGameContentVisible(true);
            }

            if (startupFailure != null)
            {
                ShowStartupAssetFailureDialog(startupFailure,
                    onOk: PresentGame);
                return;
            }

            PresentGame();
        }

        private static StartupAssetFailure CreateStartupAssetFailure(Exception exception)
        {
            var details = exception.Message;
            if (details.Length > 300)
            {
                details = details[..300] + "...";
            }

            return new StartupAssetFailure(
                "Audio could not be initialized.",
                "The game will continue without sound.",
                details);
        }

        private void UpdateStartupAssetsProgress(int loaded, int total, string statusText)
        {
            StartupAssetsLoadedCount = loaded;
            StartupAssetsTotalCount = total;
            StartupAssetsStatusText = statusText;

            OnPropertyChanged(nameof(StartupAssetsLoadedCount));
            OnPropertyChanged(nameof(StartupAssetsTotalCount));
            OnPropertyChanged(nameof(StartupAssetsStatusText));
            OnPropertyChanged(nameof(StartupAssetsProgress));
        }

        private void SetStartupAssetsOverlayVisible(bool isVisible)
        {
            if (ShowStartupAssetsOverlay == isVisible)
            {
                return;
            }

            ShowStartupAssetsOverlay = isVisible;
            OnPropertyChanged(nameof(ShowStartupAssetsOverlay));
        }

        private void SetStartupGameContentVisible(bool isVisible)
        {
            if (ShowStartupGameContent == isVisible)
            {
                return;
            }

            ShowStartupGameContent = isVisible;
        }
    }
}