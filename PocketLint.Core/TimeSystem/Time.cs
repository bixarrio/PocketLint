using System;

namespace PocketLint.Core.TimeSystem;

public static class Time
{
    #region Properties and Fields

    private static float _timeScale = 1f;
    public static float TimeScale
    {
        get => _timeScale;
        set => _timeScale = Math.Max(0f, value);
    }

    public static float UnscaledDeltaTime { get; private set; }
    public static float DeltaTime => UnscaledDeltaTime * TimeScale;

    public static float GameTime { get; private set; }

    #endregion

    #region Internal Methods

    internal static void Update(float dt)
    {
        UnscaledDeltaTime = dt;
        GameTime += UnscaledDeltaTime;
    }

    #endregion
}
