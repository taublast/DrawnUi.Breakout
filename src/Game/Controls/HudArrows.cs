using AppoMobi.Maui.Gestures;

namespace Breakout.Game;

public class HudArrows : SkiaGrid
{
    private BreakoutGame _game;

    protected SkiaHotspot HotSpotLeft;
    protected SkiaHotspot HotSpotRight;

    public HudArrows(BreakoutGame game)
    {
        _game = game;
    }

    protected override void CreateDefaultContent()
    {
        base.CreateDefaultContent();

        HotSpotLeft = new SkiaHotspot();
        HotSpotRight = new SkiaHotspot().WithColumn(1);

        AddSubView(HotSpotLeft);
        AddSubView(HotSpotRight);

        HotSpotLeft.Down += (s, args) =>
        {
            if (_game.InputPressMode)
            {
                _game.ApplyGameKey(GameKey.Left);
            }
        };

        HotSpotRight.Down += (s, args) =>
        {
            if (_game.InputPressMode)
            {
                _game.ApplyGameKey(GameKey.Right);
            }
        };
    }

    public override ISkiaGestureListener ProcessGestures(SkiaGesturesParameters args, GestureEventProcessingInfo apply)
    {
        var consumed =  base.ProcessGestures(args, apply);

        if (_game.InputPressMode)
        {
            if (consumed == null && args.Type != TouchActionResult.Up)
            {
                return this;
            }
        }

        return consumed;
    }

    public override void OnWillDisposeWithChildren()
    {
        base.OnWillDisposeWithChildren();

        _game = null;
    }
}