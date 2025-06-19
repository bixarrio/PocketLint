namespace PocketLint.Core;

public class SpriteRenderer : IComponent
{
    #region Properties and Fields

    private readonly FrameBuffer _frameBuffer;
    private readonly GameObject _gameObject;

    public GameObject GameObject => _gameObject;

    private int _spriteId;
    public int SpriteId { get => _spriteId; set => _spriteId = value; }

    #endregion

    #region ctor

    public SpriteRenderer(FrameBuffer frameBuffer, GameObject gameObject, int spriteId = 0)
    {
        _frameBuffer = frameBuffer;
        _gameObject = gameObject;
        _spriteId = spriteId;
    }

    #endregion

    #region Public Methods

    public void Update(float dt) { /* NOP */ }
    public void Draw()
    {
        var transform = _gameObject.GetComponent<Transform>();
        if (transform == null) return;
        _frameBuffer.Sprite(_spriteId, (int)transform.X, (int)transform.Y);
    }

    #endregion
}
