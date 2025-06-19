using System;

namespace PocketLint.Core;

public class PlayerController : IComponent
{
    #region Properties and Fields

    private const float SPEED = 32f;

    private readonly Input _input;
    private readonly GameObject _gameObject;

    public GameObject GameObject => _gameObject;

    #endregion

    #region ctor

    public PlayerController(Input input, GameObject gameObject)
    {
        _input = input;
        _gameObject = gameObject;
    }

    #endregion

    #region Public Methods

    public void Update(float dt)
    {
        var transform = _gameObject.GetComponent<Transform>();
        if (transform == null) return;

        if (_input.Button(0)) transform.X -= SPEED * dt;
        if (_input.Button(1)) transform.X += SPEED * dt;
        if (_input.Button(2)) transform.Y += SPEED * dt;
        if (_input.Button(3)) transform.Y -= SPEED * dt;

        transform.X = Math.Clamp(transform.X, 0, 128 - 8);
        transform.Y = Math.Clamp(transform.Y, 0, 128 - 8);
    }
    public void Draw() { /* NOP */ }

    #endregion
}
