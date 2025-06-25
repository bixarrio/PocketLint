using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PocketLint.Core.Entities;
using PocketLint.Core.Logging;
using PocketLint.Core.Rendering;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;

namespace PocketLint.Runner;

public class GameRunner
{
    #region Properties and Fields

    private readonly GameWindow _window;
    private readonly FrameBuffer _frameBuffer;
    private readonly RenderingSystem _renderer;
    private const int WIDTH = 128;
    private const int HEIGHT = 128;
    private const int WINDOW_SCALE = 4;

    #endregion

    #region ctor

    public GameRunner(GameConfig config)
    {
        var settings = new GameWindowSettings { UpdateFrequency = 60.0 };
        var nativeSettings = new NativeWindowSettings
        {
            ClientSize = new Vector2i(WIDTH * WINDOW_SCALE, HEIGHT * WINDOW_SCALE),
            Title = config.Title,
            Profile = ContextProfile.Core,
            APIVersion = new Version(3, 3)
        };
        _window = new GameWindow(settings, nativeSettings);
        var shaderLoader = new ShaderLoader();
        var textureManager = new TextureManager(WIDTH, HEIGHT);
        var entityManager = new EntityManager();
        _frameBuffer = new FrameBuffer(WIDTH, HEIGHT);
        _renderer = new RenderingSystem(shaderLoader, textureManager);
        _ = new Scene(config.StartScene, _window.KeyboardState, entityManager);

        _window.Load += OnLoad;
        _window.UpdateFrame += OnUpdate;
        _window.RenderFrame += OnRenderFrame;
        _window.Unload += OnUnload;
    }

    #endregion

    #region Public Methods

    public void Run()
    {
        Logger.Log($"Starting {_window.Title}");
        try
        {
            _window.Run();
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to run: {DumpStack(ex)}");
        }

        string DumpStack(Exception ex)
        {
            var str = $"{ex.Message}\n{ex}";
            if (ex.InnerException != null)
                str = $"{str}\n\n{DumpStack(ex.InnerException)}";
            return str;
        }
    }

    #endregion

    #region Private Methods

    private void OnLoad()
    {
        _window.MakeCurrent();
        GL.LoadBindings(new GLFWBindingsContext());
        GL.ClearColor(0f, 0f, 0f, 0f);
        GL.Viewport(0, 0, _window.ClientSize.X, _window.ClientSize.Y);
        _renderer.Initialize();
        Logger.Log($"{_window.Title} initialized (OpenGL {GL.GetString(StringName.Version)})");
    }

    private void OnUpdate(FrameEventArgs args)
    {
        // No-op for now
        Scene.Current.Update((float)args.Time);
    }

    private void OnRenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        _renderer.Render(_frameBuffer, Scene.Current);
        _window.SwapBuffers();
    }

    private void OnUnload()
    {
        _renderer.Dispose();
        Logger.Log($"Shutting down {_window.Title}");
    }

    private static void CheckGLError()
    {
        var error = GL.GetError();
        if (error != ErrorCode.NoError)
        {
            Logger.Error($"GL error: {error}");
        }
    }

    #endregion
}