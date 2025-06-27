using System.Collections;
using System.Collections.Generic;

namespace PocketLint.Core.Coroutines;

public sealed class Coroutine
{
    #region Properties and Fields

    private bool _isFinished;
    private IEnumerator _enumerator;

    public uint EntityID { get; }
    public YieldInstruction CurrentYieldInstruction { get; private set; }

    #endregion

    #region ctor

    internal Coroutine(uint entityId, IEnumerator enumerator)
    {
        EntityID = entityId;
        _enumerator = UnwrapNestedCoroutines(enumerator);
        CurrentYieldInstruction = null;
    }

    #endregion

    #region Public Methods

    internal bool MoveNext()
    {
        if (_enumerator.MoveNext())
        {
            CurrentYieldInstruction = _enumerator.Current as YieldInstruction;
            return true;
        }
        return false;
    }

    #endregion

    #region Private Methods

    private static IEnumerator UnwrapNestedCoroutines(IEnumerator root)
    {
        var stack = new Stack<IEnumerator>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var current = stack.Peek();
            if (current.MoveNext())
                if (current.Current is IEnumerator nested)
                    stack.Push(nested);
                else
                    yield return current.Current;
            else
                stack.Pop();
        }
    }

    #endregion
}
