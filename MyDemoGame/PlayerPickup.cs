using PocketLint.Core.Components;
using PocketLint.Core.Entities;
using PocketLint.Core.Logging;

namespace MyDemoGame;
public class PlayerPickup : Script
{
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
        {
            if (controller.HasKeyFor(chest.EntityId))
            {
                Logger.Log("Open chest");
            }
            else
            {
                Logger.Log("Chest is locked");
            }
        }

        var key = EntityManager.GetComponent<Key>(other.EntityId);
        if (key != null)
        {
            controller.AddKey(key);
            Scene.DestroyEntity(key.EntityId);
        }
    }
}
