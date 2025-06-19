using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PocketLint.Core;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;

namespace PocketLint.Runner;

public class Program
{
    static void Main()
    {
        var windowSettings = GameWindowSettings.Default;
        var nativeSettings = new NativeWindowSettings
        {
            ClientSize = new Vector2i(512, 512),
            Title = "PocketLint",
            Profile = ContextProfile.Core,
            APIVersion = new Version(3, 3)
        };
        using var window = new GameWindow(windowSettings, nativeSettings);
        var engine = new Engine(window);

        window.Load += () =>
        {
            GL.ClearColor(0f, 0f, 0f, 1f);
            Console.WriteLine($"OpenGL Version: {GL.GetString(StringName.Version)}");
            engine.Initialize();
        };

        window.RenderFrame += args =>
        {
            engine.Render();
            window.SwapBuffers();
        };
        window.UpdateFrame += args => engine.Update();
        window.Run();
    }
}
public class SystemTimeProvider : ITimeProvider
{
    public double GetTimeSeconds() => DateTime.UtcNow.Ticks / (double)TimeSpan.TicksPerSecond;
}
public class Engine : IKeyboardStateProvider
{
    #region Properties and Fields

    private readonly Input _input;
    private readonly Renderer _renderer;
    private readonly GameWindow _window;
    private readonly FrameBuffer _buffer = new();
    private readonly ScriptHost _scriptHost;

    private int _x = 0;
    private int _y = 64;

    #endregion

    #region ctor

    public Engine(GameWindow window)
    {
        _window = window;
        _renderer = new Renderer(_buffer);
        _input = new Input();
        _scriptHost = new ScriptHost(_buffer, _input, new SystemTimeProvider());
    }

    #endregion

    #region Public Methods

    public void Initialize() => _renderer.Initialize();

    public void Update()
    {
        _input.Update(this);
        _scriptHost.Update();
    }

    public void Render()
    {
        _scriptHost.Draw();
        _renderer.Render();
        CheckGLError("Render");
    }

    public bool IsKeyDown(Keys key) => _window.KeyboardState.IsKeyDown(key);

    #endregion

    #region Private Methods

    private void CheckGLError(string context)
    {
        var error = GL.GetError();
        if (error != ErrorCode.NoError)
            Console.WriteLine($"OpenGL Error in {context}: {error}");
    }

    #endregion
}
