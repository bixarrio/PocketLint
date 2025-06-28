using PocketLint.Core.Components;
using PocketLint.Core.Entities;
using PocketLint.Core.Logging;

namespace MyDemoGame.Scripts;
public class PlayerPickup : GameScript
{
    private Animator _animator;

    public override void Ready()
    {
        _animator = EntityManager.GetComponentInChildren<Animator>(Transform.ParentId ?? 0);
        if (_animator == null)
            Logger.Error($"Animator is null");
    }

    public override void OnTrigger(Collider other)
    {
        var player = GetComponentInParent<PlayerController>();
        if (player == null)
        {
            Logger.Error($"Could not find 'PlayerController' in parent");
            return;
        }

        var chest = EntityManager.GetComponent<Chest>(other.EntityId);
        if (chest != null && player.HasKeyFor(chest.ChestIdentifier))
        {
            player.RemoveKeyFor(chest.ChestIdentifier);
            _animator.Play(chest.GetPickupAnimation());
            Scene.DestroyEntity(chest.EntityId);
        }

        var key = EntityManager.GetComponent<Key>(other.EntityId);
        if (key != null)
        {
            player.AddKey(key);
            _animator.Play(key.GetPickupAnimation());
            Scene.DestroyEntity(key.EntityId);
        }
    }
}
