namespace PocketLint.Core.Inputs;

public interface IInputSystem
{
    bool IsButtonDown(int button);
    bool IsButtonHeld(int button);
    bool IsButtonUp(int button);
}