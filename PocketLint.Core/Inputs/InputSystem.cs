using System;

namespace PocketLint.Core.Inputs;

internal class InputSystem
{
    #region Properties and Fields

    public const int BUTTON_COUNT = 6;

    private readonly IInputProvider _inputProvider;
    private readonly bool[] _currentState;
    private readonly bool[] _previousState;

    #endregion

    #region ctor

    public InputSystem(IInputProvider inputProvider)
    {
        _inputProvider = inputProvider ?? throw new ArgumentNullException(nameof(inputProvider));
        _currentState = new bool[BUTTON_COUNT];
        _previousState = new bool[BUTTON_COUNT];
    }

    #endregion

    #region Public Methods

    public void Update() => UpdateButtonStates();

    public bool IsButtonDown(int button) => _currentState[button] && !_previousState[button];
    public bool IsButtonHeld(int button) => _currentState[button];
    public bool IsButtonUp(int button) => !_currentState[button] && _previousState[button];

    #endregion

    #region Private Methods

    private void UpdateButtonStates()
    {
        Array.Copy(_currentState, _previousState, BUTTON_COUNT);
        for (var button = 0; button < BUTTON_COUNT; button++)
            _currentState[button] = _inputProvider.IsButtonHeld(button);
    }

    #endregion
}
