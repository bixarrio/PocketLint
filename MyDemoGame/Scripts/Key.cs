using PocketLint.Core.Components;

namespace MyDemoGame.Scripts;
public class Key : GameScript
{
    public uint LinkedChestIdentifier { get; set; }

    public override void Init()
    {
        var rnd = new Random();
        Transform.SetWorldPosition(rnd.Next(0, 120), rnd.Next(0, 120));
    }
}
