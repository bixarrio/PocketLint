using PocketLint.Core.Logging;
using System;

namespace PocketLint.Core.Inputs;
public class Input
{
    #region Properties and Fields

    private const int BUTTON_COUNT = 6;

    private static IInputSystem _inputSystem;

    #endregion

    #region Public Methods

    public static void Initialize(IInputSystem inputSystem)
    {
        if (_inputSystem != null)
        {
            Logger.Warn("Input system already initialized");
            return;
        }

        _inputSystem = inputSystem;
        Logger.Log("Input system initialized");
    }

    #endregion

    #region Public Methods

    public static bool IsButtonDown(int button)
    {
        ValidateButton(button);
        return _inputSystem.IsButtonDown(button);
    }

    public static bool IsButtonHeld(int button)
    {
        ValidateButton(button);
        return _inputSystem.IsButtonHeld(button);
    }

    public static bool IsButtonUp(int button)
    {
        ValidateButton(button);
        return _inputSystem.IsButtonUp(button);
    }

    #endregion

    #region Private Methods

    private static void ValidateButton(int button)
    {
        if (button < 0 || button >= BUTTON_COUNT)
        {
            Logger.Error($"Invalid button index: {button}. Must be 0-{BUTTON_COUNT - 1}");
            throw new ArgumentException($"Invalid button index: {button}. Must be 0-{BUTTON_COUNT - 1}");
        }
    }

    #endregion
}
