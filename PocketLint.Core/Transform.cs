namespace PocketLint.Core;

public class Transform : IComponent
{
    #region Properties and Fields

    private readonly GameObject _gameObject;

    public GameObject GameObject => _gameObject;

    private float _x;
    public float X { get => _x; set => _x = value; }

    private float _y;
    public float Y { get => _y; set => _y = value; }

    #endregion

    #region ctor

    public Transform(GameObject gameObject, float x = 0f, float y = 0f)
    {
        _gameObject = gameObject;
        _x = x;
        _y = y;
    }

    #endregion

    #region Public Methods

    public void Update(float dt) { /* NOP */ }
    public void Draw() { /* NOP */ }

    #endregion
}
