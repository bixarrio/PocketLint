using PocketLint.Core.Rendering;

namespace PocketLint.Core.Components;
public abstract class Renderer : Component
{
    #region Properties and Fields

    public int Layer { get; set; } = 0;
    public int SortOrder { get; set; } = 0;

    #endregion

    #region Public Methods

    public abstract void Render(FrameBuffer frameBuffer);

    #endregion

    #region Protected Methods

    protected virtual bool ShouldCull(int x, int y, int width, int height)
    {
        return x < -SpriteSheet.SPRITE_SIZE || x >= width ||
            y < -SpriteSheet.SPRITE_SIZE || y >= height;
    }

    #endregion
}
