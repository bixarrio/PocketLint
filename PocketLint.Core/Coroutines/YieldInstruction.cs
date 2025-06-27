using System;

namespace PocketLint.Core.Coroutines;

public abstract class YieldInstruction
{
    #region Public Methods

    public abstract bool IsDone(float dt);
    public virtual void Reset() { }

    #endregion
}
public class WaitForSeconds : YieldInstruction
{
    #region Properties and Fields

    private float _duration;
    private float _remaining;

    #endregion

    #region ctor

    public WaitForSeconds(float seconds)
    {
        _duration = seconds;
        _remaining = seconds;
    }

    #endregion

    #region Public Methods

    public override bool IsDone(float dt)
    {
        _remaining -= dt;
        return _remaining <= 0f;
    }

    public override void Reset() => _remaining = _duration;

    #endregion
}
public class WaitUntil : YieldInstruction
{
    #region Properties and Fields

    private readonly Func<bool> _predicate;

    #endregion

    #region ctor

    public WaitUntil(Func<bool> predicate)
    {
        _predicate = predicate;
    }

    #endregion

    #region Public Methods

    public override bool IsDone(float dt) => _predicate.Invoke();

    #endregion
}
