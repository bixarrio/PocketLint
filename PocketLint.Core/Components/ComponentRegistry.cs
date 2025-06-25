using PocketLint.Core.Logging;
using System;
using System.Collections.Generic;

namespace PocketLint.Core.Components;

public static class ComponentRegistry
{
    #region Properties and Fields

    private static readonly Dictionary<string, Func<IComponent>> _factories = new();

    #endregion

    #region Public Methods

    public static void Initialize()
    {
        Register("Camera", () => new Camera());
        Register("Collider", () => new Collider());
        Register("EntityTransform", () => new EntityTransform());
        Register("Rigidbody", () => new Rigidbody());
        Register("SpriteRenderer", () => new SpriteRenderer());
    }

    public static void Register(string type, Func<IComponent> factory)
    {
        if (!_factories.TryAdd(type, factory))
            Logger.Error($"Duplicate component type: {type}");
    }

    public static IComponent? Create(string type)
    {
        if (_factories.TryGetValue(type, out var factory)) return factory.Invoke();
        Logger.Error($"Unknown component type: {type}");
        return null;
    }

    #endregion
}