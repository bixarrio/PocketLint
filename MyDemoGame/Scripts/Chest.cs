using PocketLint.Core.Components;
using PocketLint.Core.Data;

namespace MyDemoGame.Scripts;
public class Chest : GameScript
{
    private Animation _animation;

    public uint ChestIdentifier { get; set; }

    public override void Init()
    {
        var rnd = new Random();
        Transform.SetWorldPosition(rnd.Next(0, 120), rnd.Next(0, 120));

        _animation = new Animation("LootIndicator", [255, 70], frameRate: 6f);
    }

    public Animation GetPickupAnimation() => _animation;
}
