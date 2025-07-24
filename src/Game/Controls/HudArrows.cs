using Breakout.Game.Input;

namespace Breakout.Game;

public class HudArrows : SkiaGrid
{
    private BreakoutGame _game;
    private readonly HudController _controller;

    public HudArrows(BreakoutGame game)
    {
        _game = game;
        _controller = new HudController(game);
        _game.AddInputController(_controller);
        _game.AddInputController(new GameControllerInput(game));
    }

    public override ISkiaGestureListener ProcessGestures(SkiaGesturesParameters args, GestureEventProcessingInfo apply)
    {
        if (_game.InputPressMode)
        {
            var hitbox = this.GetHitBox();
            var consumed = _controller.ProcessGestures(args, apply, hitbox, RenderingScale);
            if (consumed)
            {
                return this;
            }
        }

        return base.ProcessGestures(args, apply);
    }

    public override void OnWillDisposeWithChildren()
    {
        base.OnWillDisposeWithChildren();

        _game = null;
    }
}