using PocketLint.Core.Components;
using PocketLint.Core.Constants;
using PocketLint.Core.Inputs;
using PocketLint.Core.TimeSystem;

namespace MyDemoGame;

public class PlayerController : Script
{
    private float _speed = 30f;
    private List<Key> _heldKeys = new();

    public override void Update()
    {
        var movement = (x: 0f, y: 0f);
        if (Input.IsButtonHeld(Button.UP)) movement.y = 1f;
        else if (Input.IsButtonHeld(Button.DOWN)) movement.y = -1f;

        if (Input.IsButtonHeld(Button.LEFT)) movement.x = -1f;
        else if (Input.IsButtonHeld(Button.RIGHT)) movement.x = 1f;

        var magnitude = (float)Math.Sqrt((movement.x * movement.x) + (movement.y * movement.y));
        if (magnitude <= 0f) return;

        movement.x /= magnitude;
        movement.y /= magnitude;
        Transform.LocalX += movement.x * _speed * Time.DeltaTime;
        Transform.LocalY += movement.y * _speed * Time.DeltaTime;
    }

    public void AddKey(Key key) => _heldKeys.Add(key);
    public bool HasKeyFor(uint chestId) => _heldKeys.Find(key => key.LinkedChestIdentifier == chestId) != null;
}
