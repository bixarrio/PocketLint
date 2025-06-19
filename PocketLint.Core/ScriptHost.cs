using System;
using System.Collections.Generic;

namespace PocketLint.Core;

public class ScriptHost
{
    #region Properties and Fields

    private const float TARGET_FRAME_TIME = 1f / 60f;

    private readonly FrameBuffer _frameBuffer;
    private readonly Input _input;
    private readonly ITimeProvider _timeProvider;
    private readonly List<GameObject> _gameObjects = new();

    private Camera _activeCamera;
    private int _nextId;
    private double _lastUpdateTime;

    #endregion

    #region ctor

    public ScriptHost(FrameBuffer frameBuffer, Input input, ITimeProvider timeProvider)
    {
        _frameBuffer = frameBuffer;
        _input = input;
        _timeProvider = timeProvider;
        _lastUpdateTime = _timeProvider.GetTimeSeconds();
        InitializeDefaultGameObjects();
    }

    #endregion

    #region Public Methods

    public GameObject CreateGameObject()
    {
        var go = new GameObject(_nextId++);
        _gameObjects.Add(go);
        return go;
    }

    public void Update()
    {
        var currentTime = _timeProvider.GetTimeSeconds();
        var dt = (float)Math.Min(currentTime - _lastUpdateTime, TARGET_FRAME_TIME * 2);
        _lastUpdateTime = currentTime;

        if (dt <= 0f) return;

        foreach (var gameObject in _gameObjects)
            gameObject.Update(dt);
    }

    public void Draw()
    {
        _frameBuffer.Clear(0);

        var offsetX = 0;
        var offsetY = 0;
        if (_activeCamera != null)
        {
            var cameraTransform = _activeCamera.GameObject.GetComponent<Transform>();
            if (cameraTransform != null)
            {
                offsetX = (int)cameraTransform.X;
                offsetY = (int)cameraTransform.Y;
            }
        }

        foreach (var gameObject in _gameObjects)
        {
            foreach (var component in gameObject.Components)
            {
                if (component is SpriteRenderer spriteRenderer)
                {
                    var transform = gameObject.GetComponent<Transform>();
                    if (transform == null) continue;
                    _frameBuffer.Sprite(spriteRenderer.SpriteId, (int)transform.X - offsetX, (int)transform.Y - offsetY);
                }
            }
        }
    }

    #endregion

    #region Private Methods

    private void InitializeDefaultGameObjects()
    {
        var playerGo = CreateGameObject();
        var playerTransform = new Transform(playerGo, 64f, 64f);
        var spriteRenderer = new SpriteRenderer(_frameBuffer, playerGo, 0);
        var playerController = new PlayerController(_input, playerGo);
        playerGo.AddComponent(playerTransform);
        playerGo.AddComponent(spriteRenderer);
        playerGo.AddComponent(playerController);

        var cameraGo = CreateGameObject();
        var cameraTransform = new Transform(cameraGo, 32f, 32f);
        var camera = new Camera(cameraGo);
        var cameraScript = new ScriptComponent(cameraGo,
            updateAction: (dt) =>
            {
                var transform = cameraGo.GetComponent<Transform>();
                if (transform == null) return;
                if (_input.Button(4)) transform.Y -= 32f * dt;
                if (_input.Button(5)) transform.Y += 32f * dt;
            });
        cameraGo.AddComponent(cameraTransform);
        cameraGo.AddComponent(camera);
        cameraGo.AddComponent(cameraScript);
        _activeCamera = camera;
    }

    #endregion
}
