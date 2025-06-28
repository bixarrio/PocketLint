using OpenTK.Windowing.GraphicsLibraryFramework;
using PocketLint.Core.Animations;
using PocketLint.Core.Components;
using PocketLint.Core.Coroutines;
using PocketLint.Core.Inputs;
using PocketLint.Core.Logging;
using PocketLint.Core.Physics;
using PocketLint.Core.Rendering;
using PocketLint.Core.Systems.GameLoopSystems;
using PocketLint.Core.TimeSystem;
using System;
using System.Collections;
using System.Linq;

namespace PocketLint.Core.Entities;

public class Scene
{
    #region Properties and Fields

    private const string DEFAULT_CAMERA_NAME = "DefaultCamera";

    private readonly string _name;
    private readonly GameLoopSystem _gameLoopSystem;
    private readonly EntityManager _entityManager;
    private readonly CoroutineSystem _coroutineSystem;

    public static Scene Current { get; private set; }
    public static Palette Palette { get; private set; }
    public static SpriteSheet SpriteSheet { get; private set; }

    public string Name => _name;
    public EntityManager EntityManager => _entityManager;

    #endregion

    #region ctor

    internal Scene(string name, KeyboardState keyboardState, EntityManager entityManager, GameLoopSystem gameLoopSystem)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Logger.Error("Scene name cannot be null or empty");
            throw new ArgumentException("Scene name cannot be null or empty", nameof(name));
        }

        _name = name;
        _entityManager = entityManager;
        _gameLoopSystem = gameLoopSystem;
        _coroutineSystem = new CoroutineSystem();

        Palette = new Palette();
        SpriteSheet = new SpriteSheet();
        Current = this;
    }

    #endregion

    #region Public Methods

    public static Scene Initialize(string name, KeyboardState keyboardState, EntityManager entityManager, GameLoopSystem gameLoopSystem)
    {
        var scene = new Scene(name, keyboardState, entityManager, gameLoopSystem);
        InitializeGameLoops(keyboardState, scene);

        var cameraId = Scene.CreateEntity(DEFAULT_CAMERA_NAME);
        Scene.AddComponent<Camera>(cameraId);

        SceneRegistry.LoadScene(name);

        return scene;
    }

    public static void LoadScene(string sceneName)
    {
        Logger.Log($"LoadScene: {sceneName}");

        foreach (var entity in Current.EntityManager.GetAllEntities())
        {
            var transform = Current.GetComponent<EntityTransform>(entity.Id)!;
            if (transform.ParentId.HasValue) continue;

            // We don't want to remove the camera, just reset it's position
            if (entity.Id == Camera.Current.EntityId)
            {
                transform.SetWorldPosition(0, 0);
                continue;
            }

            Current.EntityManager.RemoveEntity(entity.Id);
        }

        SceneRegistry.LoadScene(sceneName);
    }

    public static uint CreateEntity(string name, float x = 0f, float y = 0f, uint? parentId = null, string? tag = null)
        => Current.CreateEntityInstance(name, x, y, parentId, tag);
    public uint CreateEntityInstance(string name, float x = 0f, float y = 0f, uint? parentId = null, string? tag = null)
    {
        var entityId = _entityManager.CreateEntity(name, x, y, tag ?? "");
        if (parentId.HasValue) SetParent(entityId, parentId.Value, maintainWorldPosition: false);
        return entityId;
    }

    public void SetParent(uint childId, uint? parentId, bool maintainWorldPosition = true)
    {
        if (childId == parentId)
        {
            Logger.Warn($"Cannot parent entity ID {childId} to itself");
            return;
        }

        var childTransform = _entityManager.GetComponent<EntityTransform>(childId);
        if (childTransform == null)
        {
            Logger.Error($"Child entity ID {childId} has no TransformComponent");
            return;
        }

        // Remove from current parent
        if (childTransform.ParentId.HasValue)
        {
            var oldParentTransform = _entityManager.GetComponent<EntityTransform>(childTransform.ParentId.Value);
            oldParentTransform?.Children.Remove(childId);
        }

        // Validate new parent
        if (parentId.HasValue)
        {
            if (!_entityManager.GetAllComponents<EntityTransform>().Any(t => t.EntityId == parentId.Value))
            {
                Logger.Error($"Parent entity ID {parentId.Value} does not exist");
                return;
            }
            if (Camera.Current != null && parentId.Value == Camera.Current.EntityId)
            {
                Logger.Warn($"Cannot parent entity ID {childId} to camera entity ID {parentId.Value}");
                return;
            }
            if (IsAncestor(childId, parentId.Value))
            {
                Logger.Warn($"Cannot parent entity ID {childId} to descendant ID {parentId.Value}");
                return;
            }
        }

        // Set new parent
        childTransform.SetParent(parentId, maintainWorldPosition);
        if (parentId.HasValue)
        {
            var parentTransform = _entityManager.GetComponent<EntityTransform>(parentId.Value);
            parentTransform.Children.Add(childId);
        }

        Logger.Log($"Set parent of entity ID {childId} to {parentId?.ToString() ?? "none"}");
    }

    public static T AddComponent<T>(uint entityId) where T : IComponent, new()
        => Current.AddComponentInstance<T>(entityId);
    public T AddComponentInstance<T>(uint entityId) where T : IComponent, new()
    {
        var entity = _entityManager.GetEntity(entityId);
        if (entity == null)
        {
            Logger.Error($"Entity ID {entityId} does not exist");
            throw new ArgumentException($"Entity ID {entityId} does not exist");
        }

        try
        {
            var component = new T();
            if (component is Component concrete) concrete.Setup(entityId, _entityManager);
            _entityManager.AddComponent(entityId, component);

            // Handle special case - Camera
            if (component is Camera camera)
            {
                if (Camera.Current != null)
                    Logger.Warn($"Replacing existing camera (entity ID {Camera.Current.EntityId}) with new camera (entity ID {entityId})");
                camera.Activate();
            }

            component.Init();

            Logger.Log($"Added {typeof(T).Name} to entity ID {entityId}");
            return component;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to add component {typeof(T).Name}: {ex.Message}");
            throw new InvalidOperationException($"Failed to add component {typeof(T).Name}", ex);
        }
    }

    public T? GetComponent<T>(uint entityId) where T : class, IComponent
        => _entityManager.GetComponent<T>(entityId);

    public static void DestroyEntity(uint entityId, float delay = 0)
        => Current.DestroyEntityInstance(entityId, delay);
    public void DestroyEntityInstance(uint entityId, float delay = 0)
    {
        if (EntityManager.GetEntity(entityId) == null)
        {
            Logger.Error($"Cannot destroy non-existent entity ID {entityId}");
            return;
        }

        if (Camera.Current != null && entityId == Camera.Current.EntityId)
        {
            Logger.Warn($"Attempted to destroy camera entity ID {entityId}");
            return;
        }

        if (delay > 0)
            AddCoroutine(new Coroutine(entityId, DelayedDestroy(entityId, delay)));
        else
            PerformDestroy(entityId);
    }

    public void AddCoroutine(Coroutine coroutine) => _coroutineSystem.AddCoroutine(coroutine);
    public void RemoveCoroutine(Coroutine coroutine) => _coroutineSystem.RemoveCoroutine(coroutine);
    public void RemoveCoroutinesForEntity(uint entityId) => _coroutineSystem.RemoveCoroutinesForEntity(entityId);

    public void Update(float dt)
    {
        //Logger.Log($"Update scene '{_name}' with dt={dt}");

        Time.Update(dt);
        _gameLoopSystem.ExecuteUpdate();
    }

    #endregion

    #region Private Methods

    private static void InitializeGameLoops(KeyboardState keyboardState, Scene scene)
    {
        // Input updates first
        var inputGameLoop = new InputUpdateLoop();
        var inputSystem = new InputSystem(new KeyboardInputProvider(keyboardState));
        inputGameLoop.SubSystems.Add(inputSystem);
        scene._gameLoopSystem.AddSubSystem(inputGameLoop);
        Input.Initialize(inputSystem);

        // Player loops:
        var playerUpdateLoop = new PlayerUpdateLoop();
        // - GameScripts
        playerUpdateLoop.SubSystems.Add(new GameScriptSystem(scene.EntityManager));
        // - Physics
        playerUpdateLoop.SubSystems.Add(new PhysicsSystem(scene.EntityManager));
        // - Coroutines
        playerUpdateLoop.SubSystems.Add(scene._coroutineSystem);
        // Animations
        playerUpdateLoop.SubSystems.Add(new AnimationSystem(scene.EntityManager));

        scene._gameLoopSystem.AddSubSystem(playerUpdateLoop);
    }

    private bool IsAncestor(uint potentialAncestorId, uint entityId)
    {
        var currentId = entityId;
        while (currentId != null)
        {
            var transform = _entityManager.GetComponent<EntityTransform>(currentId);
            if (transform == null || !transform.ParentId.HasValue) return false;
            if (transform.ParentId == potentialAncestorId) return true;
            currentId = transform.ParentId.Value;
        }
        return false;
    }
    private IEnumerator DelayedDestroy(uint entityId, float delay)
    {
        yield return new WaitForSeconds(delay);
        PerformDestroy(entityId);
    }
    private void PerformDestroy(uint entityId)
    {
        RemoveCoroutinesForEntity(entityId);
        EntityManager.RemoveEntity(entityId);
    }

    #endregion
}
