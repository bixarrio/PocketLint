using PocketLint.Core.Constants;
using PocketLint.Core.Rendering;

namespace PocketLint.Core.Components;

public class SpriteRenderer : Renderer
{
    #region Properties and Fields

    public byte SpriteIndex { get; set; } = 0;
    public bool FlipX { get; set; } = false;
    public bool FlipY { get; set; } = false;
    public SpriteRotation Rotation { get; set; } = SpriteRotation.None;

    #endregion

    #region Public Methods

    public override void Render(FrameBuffer frameBuffer)
    {
        var (renderX, renderY) = CalculateRenderPosition();

        // Culling
        if (renderX < -SpriteSheet.SPRITE_SIZE || renderX >= frameBuffer.Width ||
            renderY < -SpriteSheet.SPRITE_SIZE || renderY >= frameBuffer.Height)
            return;

        frameBuffer.DrawSprite(renderX, renderY, SpriteIndex, Rotation, FlipX, FlipY);
    }

    #endregion

    #region Private Methods

    private (int x, int y) CalculateRenderPosition()
    {
        var worldX = Transform.WorldX;
        var worldY = Transform.WorldY;

        if (Camera.Current == null) return ((int)worldX, (int)worldY);

        var cameraX = Camera.Current.Transform.WorldX;
        var cameraY = Camera.Current.Transform.WorldY;
        return ((int)(worldX - cameraX), (int)(worldY - cameraY));
    }

    #endregion
}
