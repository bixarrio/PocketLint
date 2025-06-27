using PocketLint.Core.Components;
using PocketLint.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PocketLint.Core.Entities;

public class EntityManager
{
    #region Properties and Fields

    private readonly List<Entity> _entities;

    private uint _nextId;

    #endregion

    #region ctor

    public EntityManager()
    {
        _entities = new();
        _nextId = 1;
    }

    #endregion

    #region Public Methods

    public void AddComponent<T>(uint entityId, T component) where T : IComponent
    {
        if (component == null)
        {
            Logger.Error($"Cannot add null component to entity ID {entityId}");
            return;
        }

        var entity = GetEntity(entityId);
        if (entity == null)
        {
            Logger.Error($"Entity ID {entityId} not found");
            return;
        }

        entity.Components.Add(component);
        Logger.Log($"Added {typeof(T).Name} to entity '{entity.Name}'");
    }

    public T? GetComponent<T>(uint entityId) where T : class, IComponent
    {
        var entity = GetEntity(entityId);
        if (entity == null) return null;

        var component = entity.Components.OfType<T>().FirstOrDefault();
        return component;
    }
    public T? GetComponentInChildren<T>(uint entityId) where T : class, IComponent
    {
        var entity = GetEntity(entityId);
        if (entity == null)
        {
            Logger.Warn($"Entity ID {entityId} not found for GetComponentInChildren");
            return null;
        }

        Logger.Log($"Checking if this entity ({entityId}) has the component");
        var component = GetComponent<T>(entityId);
        if (component != null) return component;

        var transform = GetComponent<EntityTransform>(entityId);
        if (transform == null) return null;

        Logger.Log($"It does not. Checking if the children have the component");
        foreach (var childId in transform.Children)
        {
            Logger.Log($"Checking child with id={childId}");
            component = GetComponent<T>(childId);
            if (component != null) return component;
        }

        return null;
    }
    public T? GetComponentInParent<T>(uint entityId) where T : class, IComponent
    {
        var entity = GetEntity(entityId);
        if (entity == null)
        {
            Logger.Warn($"Entity ID {entityId} not found for GetComponentInParent");
            return null;
        }

        var component = GetComponent<T>(entityId);
        if (component != null) return component;

        var transform = GetComponent<EntityTransform>(entityId);
        if (transform == null || !transform.ParentId.HasValue) return null;

        return GetComponentInParent<T>(transform.ParentId.Value);
    }

    public IEnumerable<T> GetComponents<T>(uint entityId) where T : class, IComponent
    {
        var entity = GetEntity(entityId);
        if (entity == null)
        {
            Logger.Error($"Entity ID {entityId} not found");
            return Enumerable.Empty<T>();
        }
        return entity.Components.OfType<T>();
    }
    public IEnumerable<T> GetComponentsInChildren<T>(uint entityId) where T : class, IComponent
    {
        var entity = GetEntity(entityId);
        if (entity == null)
        {
            Logger.Warn($"Entity ID {entityId} not found for GetComponentsInChildren");
            yield break;
        }

        foreach (var component in GetComponents<T>(entityId))
            yield return component;

        var transform = GetComponent<EntityTransform>(entityId);
        if (transform == null) yield break;

        foreach (var childId in transform.Children)
            foreach (var component in GetComponents<T>(childId))
                yield return component;
    }

    public IEnumerable<(uint EntityId, T Component)> GetAllComponents<T>() where T : class, IComponent
    {
        foreach (var entity in _entities)
            foreach (var component in entity.Components.OfType<T>())
                yield return (EntityId: entity.Id, Component: component);
    }

    #endregion

    #region Internal Methods

    internal List<Entity> GetAllEntities() => _entities.ToList();

    internal uint CreateEntity(string name, float x = 0, float y = 0, string tag = "")
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Logger.Error("Cannot create entity with null or empty name");
            throw new ArgumentException("Cannot create entity with null or empty name", nameof(name));
        }

        var entity = new Entity
        {
            Id = _nextId++,
            Name = name,
            Tag = tag,
            Components = new()
        };
        var transform = new EntityTransform(x, y) { EntityId = entity.Id, EntityManager = this };
        ((IComponent)transform).Init();
        entity.Components.Add(transform);
        _entities.Add(entity);
        Logger.Log($"Created entity '{name}' (tag: '{(string.IsNullOrWhiteSpace(tag) ? "untagged" : tag)}') with ID {entity.Id} at ({x}, {y})");
        return entity.Id;
    }

    internal Entity? GetEntity(uint entityId)
    {
        foreach (var entity in _entities)
            if (entity.Id == entityId)
                return entity;
        return null;
    }

    internal void RemoveEntity(uint entityId)
    {
        var entity = GetEntity(entityId);
        if (entity == null)
        {
            Logger.Warn($"Attempted to remove non-existent entity ID {entityId}");
            return;
        }

        // Recursively destroy children
        var transform = GetComponent<EntityTransform>(entityId);
        if (transform != null)
        {
            var children = transform.Children.ToList(); // Copy to avoid modification issues
            foreach (var childId in children)
                RemoveEntity(childId);
        }

        // Notify scripts of destruction
        var components = GetComponents<IComponent>(entityId).ToList();
        foreach (var component in components)
            try
            {
                component.OnDestroy();
            }
            catch (Exception ex)
            {
                Logger.Error($"OnDestroy failed for entity ID {entityId}: {ex.Message}");
            }

        // Clear parent reference if exists
        if (transform?.ParentId.HasValue == true)
        {
            var parentTransform = GetComponent<EntityTransform>(transform.ParentId.Value);
            parentTransform?.Children.Remove(entityId);
        }

        // Remove all components
        _entities.Remove(entity);
        entity.Components.Clear();

        Logger.Log($"Removed entity '{entity.Name}' with ID {entityId}");
    }

    #endregion
}
