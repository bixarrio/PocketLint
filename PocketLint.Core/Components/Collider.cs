using PocketLint.Core.Constants;
using PocketLint.Core.Physics;

namespace PocketLint.Core.Components;

public class Collider : Component
{
    #region Properties and Fields

    public Rect Rect { get; set; }
    public uint Layer { get; set; }
    public uint Mask { get; set; }
    public bool IsTrigger { get; set; }

    public string Tag => EntityManager.GetEntity(EntityId)?.Tag;

    #endregion

    #region ctor

    public Collider()
    {
        Rect = new Rect(0f, 0f, 8f, 8f);
        Layer = CollisionLayer.Default;
        Mask = CollisionLayer.Default;
        IsTrigger = false;
    }

    #endregion
}
