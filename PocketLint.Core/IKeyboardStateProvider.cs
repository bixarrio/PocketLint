using OpenTK.Windowing.GraphicsLibraryFramework;

namespace PocketLint.Core;

public interface IKeyboardStateProvider
{
    bool IsKeyDown(Keys key);
}
