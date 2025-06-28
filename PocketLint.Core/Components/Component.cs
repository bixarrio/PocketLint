using PocketLint.Core.Entities;
using System;

namespace PocketLint.Core.Components;

public abstract class Component : IComponent
{
    #region Properties and Fields

    internal bool _hasReadied;

    private uint _entityId;
    public uint EntityId => _entityId;

    private EntityManager _entityManager;
    public EntityManager EntityManager => _entityManager;

    public EntityTransform Transform => GetTransform();

    #endregion

    #region Public Methods

    public EntityTransform GetTransform() =>
        EntityManager?.GetComponent<EntityTransform>(EntityId) ??
        throw new InvalidOperationException("EntityManager not set");

    public virtual void Init() { }
    public virtual void Ready() { }
    public virtual void Update() { }
    public virtual void OnDestroy() { }

    #endregion

    #region Internal Methods

    internal void Setup(uint entityId, EntityManager entityManager)
    {
        _entityId = entityId;
        _entityManager = entityManager;
    }

    #endregion
}
