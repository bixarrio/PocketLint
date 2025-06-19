using System.Collections.Generic;

namespace PocketLint.Core;

public class GameObject
{
    #region Properties and Fields

    private readonly int _id;
    private readonly List<IComponent> _components = new();
    public int Id => _id;

    public IReadOnlyList<IComponent> Components => _components.AsReadOnly();

    #endregion

    #region ctor

    public GameObject(int id)
    {
        _id = id;
    }

    #endregion

    #region Public Methods

    public void AddComponent(IComponent component)
    {
        _components.Add(component);
    }

    public T GetComponent<T>() where T : class, IComponent
    {
        foreach (var component in _components)
            if (component is T typedComponent)
                return typedComponent;
        return null;
    }

    public virtual void Update(float dt)
    {
        foreach (var component in _components)
            component.Update(dt);
    }

    public virtual void Draw()
    {
        foreach (var component in _components)
            component.Draw();
    }

    #endregion
}