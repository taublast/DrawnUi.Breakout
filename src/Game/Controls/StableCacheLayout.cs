namespace Breakout.Game;

public class StableCacheLayout : SkiaLayout
{
    private long _state;
    private long _stateCached;

    public override void OnScaleChanged()
    {
        _state++;

        base.OnScaleChanged();
    }

    protected override void OnCacheCreated()
    {
        base.OnCacheCreated();

        _stateCached = _state;
    }


    public override void OnChildrenChanged()
    {
        base.OnChildrenChanged();

        _state++;
    }

    public override void InvalidateCache()
    {
        if (_stateCached == _state)
        {
            return; // disable cache invalidation, we will need it built only once
        }

        base.InvalidateCache(); 
    }

}