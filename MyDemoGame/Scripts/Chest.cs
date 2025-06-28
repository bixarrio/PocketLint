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

        _animation = CreateAnimation();
    }

    public Animation GetPickupAnimation() => _animation;

    private Animation CreateAnimation()
    {
        // loot pickup animation
        var frames = Animation.FromKeyframes([(0, 255), (5, 70), (10, 255), (15, 70), (20, 255), (25, 70)], 30);
        var lootPickup = new Animation("LootIndicator", frames, frameRate: 30f, endBehaviour: AnimationEndBehaviour.Reset);
        return lootPickup;
    }
}
