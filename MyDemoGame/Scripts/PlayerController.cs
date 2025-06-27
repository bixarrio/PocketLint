using PocketLint.Core.Components;
using PocketLint.Core.Constants;
using PocketLint.Core.Coroutines;
using PocketLint.Core.Inputs;
using PocketLint.Core.Logging;
using PocketLint.Core.TimeSystem;
using System.Collections;

namespace MyDemoGame.Scripts;

public class PlayerController : GameScript
{
    private float _speed = 30f;
    private List<Key> _heldKeys = new();

    private bool _invulnerable = false;
    private const float DAMAGE_COOLDOWN = 1f;

    public override void Init()
    {
        if (Camera.Current != null) Camera.Current.ClearColorIndex = 2;
    }

    public override void Update()
    {
        var movement = (x: 0f, y: 0f);
        if (Input.IsButtonHeld(Button.UP)) movement.y = 1f;
        else if (Input.IsButtonHeld(Button.DOWN)) movement.y = -1f;

        if (Input.IsButtonHeld(Button.LEFT)) movement.x = -1f;
        else if (Input.IsButtonHeld(Button.RIGHT)) movement.x = 1f;

        var magnitude = (float)Math.Sqrt(movement.x * movement.x + movement.y * movement.y);
        if (magnitude <= 0f) return;

        movement.x /= magnitude;
        movement.y /= magnitude;
        Transform.LocalX += movement.x * _speed * Time.DeltaTime;
        Transform.LocalY += movement.y * _speed * Time.DeltaTime;
    }

    public override void OnCollision(Collider other)
    {
        if (_invulnerable) return;

        var health = GetComponentInChildren<PlayerHealth>();
        if (health == null)
        {
            Logger.Log("Could not find health component");
            return;
        }
        health.UpdateHealth(-1); // damage
        StartCoroutine(DamageCooldown(DAMAGE_COOLDOWN));
    }

    public void AddKey(Key key) => _heldKeys.Add(key);
    public bool HasKeyFor(uint chestId) => _heldKeys.Find(key => key.LinkedChestIdentifier == chestId) != null;
    public void RemoveKeyFor(uint chestId) => _heldKeys.RemoveAll(key => key.LinkedChestIdentifier == chestId);

    private IEnumerator DamageCooldown(float cooldownTime)
    {
        _invulnerable = true;
        yield return new WaitForSeconds(cooldownTime);
        _invulnerable = false;
    }
}
