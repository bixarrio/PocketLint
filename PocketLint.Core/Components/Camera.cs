﻿using PocketLint.Core.Logging;

namespace PocketLint.Core.Components;
public class Camera : Component
{
    #region Properties and Fields

    public static Camera Current { get; private set; }

    public byte ClearColorIndex { get; set; } = 0;

    #endregion

    #region Internal Methods

    internal void Activate()
    {
        Current = this;
        Logger.Log($"Camera activated for entity ID {EntityId}");
    }

    #endregion
}
