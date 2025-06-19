using OpenTK.Mathematics;

namespace PocketLint.Core.Tests;

public class FrameBufferTests
{
    [Fact]
    public void SetPixel_ValidCoordinates_SetsCorrectColor()
    {
        var buffer = new FrameBuffer();
        buffer.SetPixel(10, 10, 8);
        var color = buffer.GetPixel(10, 10);
        Assert.Equal(new Color4(0.94f, 0.29f, 0.27f, 1f), color);
    }

    [Fact]
    public void SetPixel_InvalidCoordinates_IgnoresSet()
    {
        var buffer = new FrameBuffer();
        buffer.SetPixel(-1, 0, 8);
        buffer.SetPixel(128, 0, 8);
        Assert.Equal(Color4.Black, buffer.GetPixel(0, 0));
    }

    [Fact]
    public void Clear_SetsAllPixelsToPaletteColor()
    {
        var buffer = new FrameBuffer();
        buffer.SetPixel(10, 10, 8);
        buffer.Clear(12);
        for (int y = 0; y < buffer.Height; y++)
            for (int x = 0; x < buffer.Width; x++)
                Assert.Equal(new Color4(0.26f, 0.62f, 0.8f, 1f), buffer.GetPixel(x, y));
    }

    [Fact]
    public void Sprite_ValidIdAndPosition_DrawsSpriteWithBlack()
    {
        var buffer = new FrameBuffer();
        buffer.Clear(12);
        buffer.Sprite(0, 10, 10);
        var redColor = buffer.GetPixel(13, 13);
        var blackColor = buffer.GetPixel(12, 12);
        var transparentColor = buffer.GetPixel(10, 10);
        Assert.Equal(new Color4(0.94f, 0.29f, 0.27f, 1f), redColor);
        Assert.Equal(new Color4(0f, 0f, 0f, 1f), blackColor);
        Assert.Equal(new Color4(0.26f, 0.62f, 0.8f, 1f), transparentColor);
    }

    [Fact]
    public void SetSpritePixel_ValidCoordinates_SetsCorrectPaletteIndex()
    {
        var buffer = new FrameBuffer();
        buffer.SetSpritePixel(1, 2, 2, 12);
        buffer.Sprite(1, 10, 10);
        var color = buffer.GetPixel(12, 12);
        Assert.Equal(new Color4(0.26f, 0.62f, 0.8f, 1f), color);
    }

    [Fact]
    public void SetSpritePixel_TransparentPixel_SkipsDrawing()
    {
        var buffer = new FrameBuffer();
        buffer.Clear(12);
        buffer.SetSpritePixel(1, 2, 2, 8, true);
        buffer.SetSpritePixel(1, 3, 3, 8, false);
        buffer.Sprite(1, 10, 10);
        Assert.Equal(new Color4(0.26f, 0.62f, 0.8f, 1f), buffer.GetPixel(12, 12));
        Assert.Equal(new Color4(0.94f, 0.29f, 0.27f, 1f), buffer.GetPixel(13, 13));
    }
}