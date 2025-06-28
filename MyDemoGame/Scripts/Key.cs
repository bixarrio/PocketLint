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
        _animation = CreateAnimation();
    }

    public Animation GetPickupAnimation() => _animation;

    #region Private Methods

    private Animation CreateAnimation()
    {
        // key pickup animation
        var frames = Animation.FromKeyframes([(0, 255), (5, 90), (10, 255), (15, 90), (20, 255), (25, 90)], 30);
        var keyPickup = new Animation("KeyIndicator", frames, frameRate: 30f, endBehaviour: AnimationEndBehaviour.Reset);
        return keyPickup;
    }

    #endregion
}
