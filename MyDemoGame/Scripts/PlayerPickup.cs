using PocketLint.Core.Components;
using PocketLint.Core.Entities;
using PocketLint.Core.Logging;

namespace MyDemoGame.Scripts;
public class PlayerPickup : GameScript
{
    private PickupIndicator _indicator;

    public override void Ready()
    {
        _indicator = EntityManager.GetComponentInChildren<PickupIndicator>(Transform.ParentId.Value);
        if (_indicator == null)
            Logger.Error($"PickupIndicator is null");
    }

    public override void OnTrigger(Collider other)
    {
        var controller = GetComponentInParent<PlayerController>();
        if (controller == null)
        {
            Logger.Error($"Could not find 'PlayerController' in parent");
            return;
        }

        var chest = EntityManager.GetComponent<Chest>(other.EntityId);
        if (chest != null)
            if (controller.HasKeyFor(chest.ChestIdentifier))
            {
                controller.RemoveKeyFor(chest.ChestIdentifier);
                _indicator.ShowPickup(70, 1f); // sword for now
                Scene.DestroyEntity(chest.EntityId);
            }

        var key = EntityManager.GetComponent<Key>(other.EntityId);
        if (key != null)
        {
            controller.AddKey(key);
            _indicator.ShowPickup(GetSpriteId(key.EntityId), 1f);
            Scene.DestroyEntity(key.EntityId);
        }
    }

    private byte GetSpriteId(uint entityId)
    {
        var renderer = EntityManager.GetComponent<SpriteRenderer>(entityId);
        if (renderer == null) return 255;
        return renderer.SpriteIndex;
    }
}
