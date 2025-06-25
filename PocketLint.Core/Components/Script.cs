using PocketLint.Core.Coroutines;
using PocketLint.Core.Entities;
using System.Collections;
using System.Collections.Generic;

namespace PocketLint.Core.Components;

public abstract class Script : Component
{
    #region Properties and Fields

    internal bool _hasStarted;

    #endregion

    #region Public Methods

    public virtual void OnCollision(Collider other) { }
    public virtual void OnTrigger(Collider other) { }

    public Coroutine StartCoroutine(IEnumerator routine)
    {
        var coroutine = new Coroutine(EntityId, routine);
        Scene.Current.AddCoroutine(coroutine);
        return coroutine;
    }
    public void StopCoroutine(Coroutine coroutine)
    {
        if (coroutine.EntityID == EntityId)
            Scene.Current.RemoveCoroutine(coroutine);
    }
    public void StopAllCoroutines()
    {
        Scene.Current.RemoveCoroutinesForEntity(EntityId);
    }

    // Helpers
    public T? GetComponent<T>() where T : class, IComponent => EntityManager.GetComponent<T>(EntityId);
    public T? GetComponentInChildren<T>() where T : class, IComponent => EntityManager.GetComponentInChildren<T>(EntityId);
    public T? GetComponentInParent<T>() where T : class, IComponent => EntityManager.GetComponentInParent<T>(EntityId);

    public IEnumerable<T> GetComponents<T>() where T : class, IComponent => EntityManager.GetComponents<T>(EntityId);
    public IEnumerable<T> GetComponentsInChildren<T>() where T : class, IComponent => EntityManager.GetComponentsInChildren<T>(EntityId);

    #endregion
}
