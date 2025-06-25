using PocketLint.Core.Logging;
using System;
using System.Collections.Generic;

namespace PocketLint.Core.Entities;

public static class SceneRegistry
{
    #region Properties and Fields

    private static readonly Dictionary<string, Action> _factories = new();

    #endregion

    #region Public Methods

    public static void Initialize()
    {
        // Maybe??
    }

    public static void Register(string sceneName, Action factory)
    {
        if (!_factories.TryAdd(sceneName, factory))
            Logger.Error($"Duplicate scene name: {sceneName}");
    }

    public static void LoadScene(string sceneName)
    {
        if (!_factories.TryGetValue(sceneName, out var factory))
        {
            Logger.Error($"Unknown scene: {sceneName}");
            return;
        }
        factory.Invoke();
    }

    #endregion
}
