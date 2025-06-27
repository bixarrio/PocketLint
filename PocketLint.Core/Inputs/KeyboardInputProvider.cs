using OpenTK.Windowing.GraphicsLibraryFramework;
using PocketLint.Core.Logging;

namespace PocketLint.Core.Inputs;
public class KeyboardInputProvider : IInputProvider
{
    #region Properties and Fields

    private readonly KeyboardState _keyboardState;

    private readonly Keys[] _buttonTokeyMap = new Keys[]
    {
        Keys.W,
        Keys.S,
        Keys.A,
        Keys.D,
        Keys.Space,
        Keys.LeftShift,
    };

    #endregion

    #region ctor

    public KeyboardInputProvider(KeyboardState keyboardState) => _keyboardState = keyboardState;

    #endregion

    #region Public Methods

    public bool IsButtonHeld(int button)
    {
        if (button < 0 || button >= _buttonTokeyMap.Length)
        {
            Logger.Error($"Invalid button index: {button}. Must be 0-{_buttonTokeyMap.Length - 1}");
            return false;
        }
        return _keyboardState.IsKeyDown(_buttonTokeyMap[button]);
    }

    #endregion
}
