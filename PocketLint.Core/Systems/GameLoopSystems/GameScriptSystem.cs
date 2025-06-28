using PocketLint.Core.Components;
using PocketLint.Core.Entities;
using PocketLint.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PocketLint.Core.Systems.GameLoopSystems;
public class GameScriptSystem : ISubSystem
{
    #region Properties and Fields

    private readonly EntityManager _entityManager;

    public List<ISubSystem> SubSystems { get; set; } = new();

    #endregion

    #region ctor

    public GameScriptSystem(EntityManager entityManager)
    {
        _entityManager = entityManager;
    }

    #endregion

    #region Public Methods

    public void Update()
    {
        var scriptsList = _entityManager.GetAllComponents<GameScript>().ToList();
        foreach (var (entityId, script) in scriptsList)
            try
            {
                if (!script._hasStarted)
                {
                    script.Ready();
                    script._hasStarted = true;
                }
                script.Update();
            }
            catch (Exception ex)
            {
                Logger.Error($"Script lifecycle failed for entity ID {entityId}: {ex.Message}");
            }
    }

    #endregion
}
