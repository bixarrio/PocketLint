using PocketLint.Core.Components;
using PocketLint.Core.Entities;
using PocketLint.Core.Logging;
using PocketLint.Core.Systems;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PocketLint.Core.Animations;
internal class AnimationSystem : ISubSystem
{
    #region Properties and Fields

    private readonly EntityManager _entityManager;

    public List<ISubSystem> SubSystems { get; set; } = new();

    #endregion

    #region ctor

    public AnimationSystem(EntityManager entityManager)
    {
        _entityManager = entityManager;
    }

    #endregion

    #region Public Methods

    public void Update()
    {
        var animatorsList = _entityManager.GetAllComponents<Animator>().ToList();
        foreach (var (entityId, animator) in animatorsList)
            try
            {
                if (!animator._hasReadied)
                {
                    animator.Ready();
                    animator._hasReadied = true;
                }
                animator.Update();
            }
            catch (Exception ex)
            {
                Logger.Error($"Animator lifecycle failed for entity ID {entityId}: {ex.Message}");
            }
    }

    #endregion
}
