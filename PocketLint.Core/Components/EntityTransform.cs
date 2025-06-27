using PocketLint.Core.Entities;
using System.Collections.Generic;

namespace PocketLint.Core.Components;

public class EntityTransform : IComponent
{
    #region Properties and Fields

    public uint EntityId { get; internal set; }
    public EntityManager EntityManager { get; internal set; }
    public EntityTransform Transform => this;

    public uint? ParentId { get; private set; }
    public List<uint> Children { get; } = new List<uint>();
    public float LocalX { get; set; }
    public float LocalY { get; set; }
    public float WorldX => CalculateWorldX();
    public float WorldY => CalculateWorldY();

    #endregion

    #region ctor

    internal EntityTransform(float x = 0, float y = 0)
    {
        LocalX = x;
        LocalY = y;
    }

    #endregion

    #region Public Methods

    public (float x, float y) TransformPoint(float localX, float localY) => (WorldX + localX, WorldY + localY);

    public (float x, float y) TransformPointInverse(float worldX, float worldY)
    {
        var parentTransform = ParentId.HasValue ? EntityManager.GetComponent<EntityTransform>(ParentId.Value) : null;
        var (parentWorldX, parentWorldY) = parentTransform?.TransformPoint(0f, 0f) ?? (0f, 0f);
        return (worldX - parentWorldX, worldY - parentWorldY);
    }

    public void SetWorldPosition(float worldX, float worldY)
    {
        var (localX, localY) = TransformPointInverse(worldX, worldY);
        LocalX = localX;
        LocalY = localY;
    }

    #endregion

    #region Internal Methods

    internal void SetParent(uint? parentId, bool maintainWorldPosition)
    {
        if (maintainWorldPosition)
        {
            var worldX = WorldX;
            var worldY = WorldY;
            ParentId = parentId;
            var (parentX, parentY) = parentId.HasValue
                ? EntityManager.GetComponent<EntityTransform>(parentId.Value)?.TransformPoint(0f, 0f) ?? (0f, 0f)
                : (0f, 0f);
            LocalX = worldX - parentX;
            LocalY = worldY - parentY;
        }
        else
            ParentId = parentId;
    }

    #endregion

    #region Private Methods

    private float CalculateWorldX()
    {
        if (!ParentId.HasValue)
            return LocalX;

        var parentTransform = EntityManager.GetComponent<EntityTransform>(ParentId.Value);
        if (parentTransform == null)
            return LocalX;

        var (parentWorldX, _) = parentTransform.TransformPoint(0f, 0f);
        return parentWorldX + LocalX;
    }

    private float CalculateWorldY()
    {
        if (!ParentId.HasValue)
            return LocalY;

        var parentTransform = EntityManager.GetComponent<EntityTransform>(ParentId.Value);
        if (parentTransform == null)
            return LocalY;

        var (_, parentWorldY) = parentTransform.TransformPoint(0f, 0f);
        return parentWorldY + LocalY;
    }

    #endregion

    #region Explicit Methods

    void IComponent.Init() { }
    void IComponent.Ready() { }
    void IComponent.Update() { }
    void IComponent.OnDestroy() { }

    #endregion
}