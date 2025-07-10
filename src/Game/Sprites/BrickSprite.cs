﻿using SkiaSharp;

namespace Breakout.Game;

public class BrickSprite : SkiaShape, IWithHitBox, IReusableSprite
{
    public static BrickSprite Create()
    {
        return new BrickSprite
        {
            BackgroundColor = Colors.Red,
            CornerRadius = 6,
            WidthRequest = 50,
            HeightRequest = 24,
            StrokeColor = Colors.White,
            StrokeWidth = 2,
            BevelType = BevelType.Bevel,
            Bevel = new SkiaBevel()
            {
                Depth = 4,
                LightColor = Colors.White,
                ShadowColor = Color.Parse("#333333"),
                Opacity = 0.33
            },
            UseCache = SkiaCacheType.Image,
            ZIndex = 4
        };
    }

    //public Guid Uid { get; } = Guid.NewGuid();

    public bool IsActive { get; set; }

    public void ResetAnimationState()
    {
        try { this.CancelDisappearing?.Cancel(); }
        catch {  }      
        Opacity = 1;
        Scale = 1;
    }

    /// <summary>
    /// Cancellation token for sprite removal animations.
    /// </summary>
    public CancellationTokenSource CancelDisappearing { get; set; }  

    public async Task AnimateDisappearing()
    {
        this.CancelDisappearing?.Cancel();
        using var cancel = new CancellationTokenSource();
        CancelDisappearing = cancel;
        await FadeToAsync(0, 150, Easing.SpringOut, cancel);
    }

    public void UpdateState(long time)
    {
        if (_stateUpdated != time)
        {
            HitBox = this.GetHitBox();
            _stateUpdated = time;
        }
    }
    private long _stateUpdated;

    public SKRect HitBox { get; private set; }

    /// <summary>
    /// By default it's 0 means 1 hit required to destroy
    /// </summary>
    public int SupplementaryHitsToDestroy { get; set; }

    /// <summary>
    /// By default it's false
    /// </summary>
    public bool Undestructible { get; set; }

}

