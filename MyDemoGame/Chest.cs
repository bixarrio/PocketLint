using PocketLint.Core.Components;

namespace MyDemoGame;
public class Chest : Script
{
    public uint ChestIdentifier { get; set; }

    public override void Init()
    {
        var rnd = new Random();
        Transform.SetWorldPosition(rnd.Next(0, 128), rnd.Next(0, 128));
    }
}
