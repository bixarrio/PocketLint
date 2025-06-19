using OpenTK.Windowing.GraphicsLibraryFramework;

namespace PocketLint.Core;

public class Input
{
    #region Properties and Fields

    private readonly bool[] _buttonStates = new bool[6];

    #endregion

    #region Public Methods

    public void Update(IKeyboardStateProvider keyboardStateProvider)
    {
        _buttonStates[0] = keyboardStateProvider.IsKeyDown(Keys.Left);
        _buttonStates[1] = keyboardStateProvider.IsKeyDown(Keys.Right);
        _buttonStates[2] = keyboardStateProvider.IsKeyDown(Keys.Up);
        _buttonStates[3] = keyboardStateProvider.IsKeyDown(Keys.Down);
        _buttonStates[4] = keyboardStateProvider.IsKeyDown(Keys.Z);
        _buttonStates[5] = keyboardStateProvider.IsKeyDown(Keys.X);
    }

    public virtual bool Button(int index)
    {
        if (index < 0 || index >= _buttonStates.Length) return false;
        return _buttonStates[index];
    }

    #endregion
}
