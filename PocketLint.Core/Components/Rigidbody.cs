namespace PocketLint.Core.Components;

public class Rigidbody : Component
{
    #region Properties and Fields

    public float VelocityX { get; set; }
    public float VelocityY { get; set; }

    #endregion

    #region ctor

    public Rigidbody()
    {
        VelocityX = 0f;
        VelocityY = 0f;
    }

    #endregion
}
