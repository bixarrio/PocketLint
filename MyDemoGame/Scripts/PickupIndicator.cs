using PocketLint.Core.Components;
using PocketLint.Core.TimeSystem;

namespace MyDemoGame.Scripts;
public class PickupIndicator : GameScript
{
    private float _timer;
    private bool _isActive;
    private SpriteRenderer _spriteRenderer;
    public override void Init()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.SpriteIndex = 255; // last sprite should be empty
        _spriteRenderer.SortOrder = 5;
    }
    public override void Update()
    {
        if (!_isActive) return;

        _timer -= Time.DeltaTime;
        if (_timer > 0) return;

        _isActive = false;
        _spriteRenderer.SpriteIndex = 255;
    }

    public void ShowPickup(byte spriteIndex, float showTime)
    {
        _isActive = true;
        _timer = showTime;
        _spriteRenderer.SpriteIndex = spriteIndex;
    }
}
