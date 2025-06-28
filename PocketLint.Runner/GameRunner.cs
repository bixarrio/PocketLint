using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PocketLint.Core.Components;
using PocketLint.Core.Entities;
using PocketLint.Core.Logging;
using PocketLint.Core.Rendering;
using PocketLint.Core.Systems.GameLoopSystems;
using PocketLint.Core.TimeSystem;
using PocketLint.Tools;
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

        var gameLoopSystem = new GameLoopSystem();

        SceneRegistry.Register("splashScene", () => SplashSceneSetup(config.StartScene));
        Scene.Initialize("splashScene", _window.KeyboardState, entityManager, gameLoopSystem);

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

    private void SplashSceneSetup(string startScene)
    {
        var id = Scene.CreateEntity("Splash");
        Scene.AddComponent<SplashRenderer>(id);
        Scene.AddComponent<GameSceneLoader>(id).GameSceneName = startScene;
    }

    #endregion

    #region Classes and Structs

    class SplashRenderer : Renderer
    {
        byte[] _splashImage;

        public override void Init()
        {
            _splashImage = GetSplashBytes();
        }

        public override void Render(FrameBuffer frameBuffer)
        {
            frameBuffer.DrawToScreen(_splashImage, 0, (uint)_splashImage.Length);
        }

        private byte[] GetSplashBytes()
        {
            var assembly = GetType().Assembly;
            using var stream = assembly.GetManifestResourceStream($"PocketLint.Runner.Resources.pl.png");
            if (stream == null)
            {
                Logger.Error("No embedded resources found");
                var resources = assembly.GetManifestResourceNames() ?? Array.Empty<string>();
                Logger.Log($"Resources available: {string.Join(", ", resources)}");
                return new byte[0];
            }
            return ImageImporter.ImportStream(stream, flipY: true);
        }
    }
    class GameSceneLoader : GameScript
    {
        float _timer = 3f;
        public string GameSceneName { get; set; }

        public override void Update()
        {
            _timer -= Time.DeltaTime;
            if (_timer <= 0)
            {
                Logger.Log($"Splash time over. Load game scene '{GameSceneName}'");
                Scene.LoadScene(GameSceneName);
            }
        }
    }

    #endregion
}