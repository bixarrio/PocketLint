using PocketLint.Core.Coroutines;
using PocketLint.Core.Logging;
using PocketLint.Core.TimeSystem;
using System;
using System.Collections.Generic;

namespace PocketLint.Core.Systems.GameLoopSystems;
internal class CoroutineSystem : ISubSystem
{
    #region Properties and Fields

    private readonly List<Coroutine> _activeCoroutines = new();

    public List<ISubSystem> SubSystems { get; set; } = new();

    #endregion

    #region Public Methods

    public void AddCoroutine(Coroutine coroutine) => _activeCoroutines.Add(coroutine);
    public void RemoveCoroutine(Coroutine coroutine) => _activeCoroutines.Remove(coroutine);
    public void RemoveCoroutinesForEntity(uint entityId) => _activeCoroutines.RemoveAll(c => c.EntityID == entityId);

    public void Update()
    {
        for (var i = _activeCoroutines.Count - 1; i >= 0; i--)
        {
            var coroutine = _activeCoroutines[i];
            if (!AdvanceCoroutine(coroutine, Time.DeltaTime))
                _activeCoroutines.Remove(coroutine);
        }
    }

    #endregion

    #region Private Methods

    private static bool AdvanceCoroutine(Coroutine coroutine, float dt)
    {
        try
        {
            if (coroutine.CurrentYieldInstruction == null)
                return coroutine.MoveNext();

            if (!coroutine.CurrentYieldInstruction.IsDone(dt))
                return true;

            coroutine.CurrentYieldInstruction.Reset();

            return coroutine.MoveNext();
        }
        catch (Exception ex)
        {
            Logger.Error($"Coroutine failed for entity ID: {coroutine.EntityID}: {ex.Message}");
            return false;
        }
    }
    #endregion
}
