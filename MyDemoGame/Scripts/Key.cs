using PocketLint.Core.Components;
using PocketLint.Core.Data;

namespace MyDemoGame.Scripts;
public class Key : GameScript
{
    private Animation _animation;

    public uint LinkedChestIdentifier { get; set; }

    public override void Init()
    {
        var rnd = new Random();
        Transform.SetWorldPosition(rnd.Next(0, 120), rnd.Next(0, 120));

        _animation = new Animation("KeyIndicator", [255, 90], frameRate: 6f);
    }

    public Animation GetPickupAnimation() => _animation;
}
