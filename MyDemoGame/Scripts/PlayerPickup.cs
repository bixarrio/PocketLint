using PocketLint.Core.Components;
using PocketLint.Core.Coroutines;
using PocketLint.Core.Data;
using PocketLint.Core.Entities;
using PocketLint.Core.Logging;
using System.Collections;

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
            StopAllCoroutines();
            StartCoroutine(PickupFlash(chest.GetPickupAnimation(), 1f));
            Scene.DestroyEntity(chest.EntityId);
        }

        var key = EntityManager.GetComponent<Key>(other.EntityId);
        if (key != null)
        {
            player.AddKey(key);
            StopAllCoroutines();
            StartCoroutine(PickupFlash(key.GetPickupAnimation(), 1f));
            Scene.DestroyEntity(key.EntityId);
        }
    }

    private IEnumerator PickupFlash(Animation animation, float duration)
    {
        _animator.Play(animation);
        yield return new WaitForSeconds(duration);
        _animator.Stop();
    }
}
