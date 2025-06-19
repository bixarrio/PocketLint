using System;

namespace PocketLint.Core;

public class ScriptComponent : IComponent
{
    #region Properties and Fields

    private readonly GameObject _gameObject;
    private readonly Action<float> _updateAction;
    private readonly Action _drawAction;

    public GameObject GameObject => _gameObject;

    #endregion

    #region ctor

    public ScriptComponent(GameObject gameObject, Action<float>? updateAction = null, Action? drawAction = null)
    {
        _gameObject = gameObject;
        _updateAction = updateAction ?? ((_) => { /* NOP */  });
        _drawAction = drawAction ?? (() => { /* NOP */  });
    }

    #endregion

    #region Public Methods

    public void Update(float dt) => _updateAction.Invoke(dt);
    public void Draw() => _drawAction.Invoke();

    #endregion
}
