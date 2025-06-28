using PocketLint.Core.Components;
using PocketLint.Core.Entities;
using PocketLint.Core.Logging;
using PocketLint.Core.Systems;
using PocketLint.Core.TimeSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PocketLint.Core.Physics;

internal class PhysicsSystem : ISubSystem
{
    #region Properties and Fields

    private readonly EntityManager _entityManager;

    public List<ISubSystem> SubSystems { get; set; } = new();

    #endregion

    #region ctor

    public PhysicsSystem(EntityManager entityManager)
    {
        _entityManager = entityManager ?? throw new ArgumentNullException(nameof(entityManager));
        Logger.Log("PhysicsSystem initialized");
    }

    #endregion

    #region Public Methods

    public void Update()
    {
        UpdateRigidbodies();
        ResolveSolidCollisions();
        ResolveTriggerCollisions();
    }

    #endregion

    #region Private Methods

    private void UpdateRigidbodies()
    {
        foreach (var (entityId, rb) in _entityManager.GetAllComponents<Rigidbody>())
        {
            var transform = _entityManager.GetComponent<EntityTransform>(entityId);
            if (transform == null) continue;

            var (worldX, worldY) = transform.TransformPoint(0f, 0f);
            worldX += rb.VelocityX * Time.DeltaTime;
            worldY += rb.VelocityY * Time.DeltaTime;
            transform.SetWorldPosition(worldX, worldY);
        }
    }

    private void ResolveSolidCollisions()
    {
        var colliders = _entityManager.GetAllComponents<Collider>().ToList();
        for (var i = 0; i < colliders.Count; i++)
        {
            var (idA, a) = colliders[i];
            if (a.IsTrigger) continue;

            for (var j = i + 1; j < colliders.Count; j++)
            {
                var (idB, b) = colliders[j];
                if (b.IsTrigger || !CanCollide(a, b)) continue;

                if (TryGetOverlap(a, b, out var bounds, out var overlapX, out var overlapY))
                    HandleSolidCollision(idA, a, idB, b, bounds, overlapX, overlapY);
            }
        }
    }

    private void ResolveTriggerCollisions()
    {
        var colliders = _entityManager.GetAllComponents<Collider>().ToList();
        for (var i = 0; i < colliders.Count; i++)
        {
            var (idA, a) = colliders[i];
            if (!a.IsTrigger) continue;

            for (var j = 0; j < colliders.Count; j++)
            {
                if (i == j) continue;
                var (idB, b) = colliders[j];
                if (!CanCollide(a, b)) continue;

                if (TryGetOverlap(a, b, out _, out _, out _))
                    NotifyTrigger(idA, b);
            }
        }
    }

    private bool CanCollide(Collider a, Collider b)
    {
        return (a.Layer & b.Mask) != 0 && (b.Layer & a.Mask) != 0;
    }

    private bool TryGetOverlap(Collider a, Collider b, out (float minAX, float maxAX, float minAY, float maxAY, float minBX, float maxBX, float minBY, float maxBY) bounds, out float overlapX, out float overlapY)
    {
        overlapX = 0f;
        overlapY = 0f;
        bounds = (0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);

        var transformA = _entityManager.GetComponent<EntityTransform>(a.EntityId);
        var transformB = _entityManager.GetComponent<EntityTransform>(b.EntityId);
        if (transformA == null || transformB == null) return false;

        var worldPosA = transformA.TransformPoint(a.Rect.X, a.Rect.Y);
        var worldPosB = transformB.TransformPoint(b.Rect.X, b.Rect.Y);

        var minAX = worldPosA.x;
        var maxAX = worldPosA.x + a.Rect.Width;
        var minAY = worldPosA.y;
        var maxAY = worldPosA.y + a.Rect.Height;

        var minBX = worldPosB.x;
        var maxBX = worldPosB.x + b.Rect.Width;
        var minBY = worldPosB.y;
        var maxBY = worldPosB.y + b.Rect.Height;

        if (maxAX <= minBX || maxBX <= minAX || maxAY <= minBY || maxBY <= minAY)
            return false;

        overlapX = Math.Min(maxAX, maxBX) - Math.Max(minAX, minBX);
        overlapY = Math.Min(maxAY, maxBY) - Math.Max(minAY, minBY);
        bounds = (minAX, maxAX, minAY, maxAY, minBX, maxBX, minBY, maxBY);
        return true;
    }

    private void HandleSolidCollision(uint idA, Collider a, uint idB, Collider b, (float minAX, float maxAX, float minAY, float maxAY, float minBX, float maxBX, float minBY, float maxBY) bounds, float overlapX, float overlapY)
    {
        var rbA = _entityManager.GetComponent<Rigidbody>(idA);
        var rbB = _entityManager.GetComponent<Rigidbody>(idB);
        if (rbA == null && rbB == null) return;

        var transformA = _entityManager.GetComponent<EntityTransform>(idA);
        var transformB = _entityManager.GetComponent<EntityTransform>(idB);
        if (transformA == null || transformB == null) return;

        var xDirection = GetPushDirection(bounds.minAX, bounds.minBX, rbA?.VelocityX ?? 0f, rbB?.VelocityX ?? 0f);
        var yDirection = GetPushDirection(bounds.minAY, bounds.minBY, rbA?.VelocityY ?? 0f, rbB?.VelocityY ?? 0f);

        var pushX = overlapY < overlapX ? 0f : overlapX * xDirection;
        var pushY = overlapY < overlapX ? overlapY * yDirection : 0f;

        if (rbA != null)
        {
            var (worldX, worldY) = transformA.TransformPoint(0f, 0f);
            var pushFactor = rbB != null ? 0.5f : 1f;
            transformA.SetWorldPosition(worldX - pushX * pushFactor, worldY - pushY * pushFactor);
        }

        if (rbB != null)
        {
            var (worldX, worldY) = transformB.TransformPoint(0f, 0f);
            var pushFactor = rbA != null ? 0.5f : 1f;
            transformB.SetWorldPosition(worldX + pushX * pushFactor, worldY + pushY * pushFactor);
        }

        NotifyCollision(idA, b);
        NotifyCollision(idB, a);

        float GetPushDirection(float posA, float posB, float velA, float velB)
        {
            if (posA != posB) return posA < posB ? 1f : -1f;
            return velA > velB ? 1f : -1f;
        }
    }

    private void NotifyTrigger(uint entityId, Collider other)
    {
        Logger.Log($"Trigger collision: entity {entityId} with {other.EntityId} (tag: {other.Tag})");
        foreach (var script in _entityManager.GetComponents<GameScript>(entityId))
            try
            {
                script.OnTrigger(other);
            }
            catch (Exception ex)
            {
                Logger.Error($"OnTrigger failed for entity {entityId}: {ex.Message}");
            }
    }

    private void NotifyCollision(uint entityId, Collider other)
    {
        Logger.Log($"Solid collision: entity {entityId} with {other.EntityId} (tag: {other.Tag})");
        foreach (var script in _entityManager.GetComponents<GameScript>(entityId))
            try
            {
                script.OnCollision(other);
            }
            catch (Exception ex)
            {
                Logger.Error($"OnCollision failed for entity {entityId}: {ex.Message}");
            }
    }

    #endregion
}