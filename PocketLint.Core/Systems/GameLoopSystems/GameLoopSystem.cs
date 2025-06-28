using PocketLint.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PocketLint.Core.Systems.GameLoopSystems;
public class GameLoopSystem
{
    #region Properties and Fields

    private readonly List<Action> _updateSystems = new();

    #endregion

    #region Public Methods

    public void AddSubSystem(ISubSystem system)
    {
        if (system == null)
        {
            Logger.Error($"SubSystem cannot be null");
            return;
        }
        _updateSystems.Add(() => UpdateSystem(system));
    }

    public void ExecuteUpdate()
    {
        foreach (var system in _updateSystems)
            system?.Invoke();
    }

    #endregion

    #region Private Methods

    private void UpdateSystem(ISubSystem system)
    {
        system.Update();
        foreach (var subSystem in system.SubSystems ?? Enumerable.Empty<ISubSystem>())
            UpdateSystem(subSystem);
    }

    #endregion
}
