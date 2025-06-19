namespace PocketLint.Core;

public class Camera : IComponent
{
    #region Proeprties and Fields

    private readonly GameObject _gameObject;
    public GameObject GameObject => _gameObject;

    #endregion

    #region ctor

    public Camera(GameObject gameObject) => _gameObject = gameObject;

    #endregion

    #region Public Methods

    public void Update(float dt) { /* NOP */ }
    public void Draw() { /* NOP */ }

    #endregion
}
