using PocketLint.Core.Constants;
using PocketLint.Core.Entities;
using PocketLint.Core.Logging;
using System;

namespace PocketLint.Core.Rendering;

public class FrameBuffer
{
    #region Properties and Fields

    private readonly int _width;
    private readonly int _height;
    private readonly byte[] _pixels;

    public int Width => _width;
    public int Height => _height;

    #endregion

    #region ctor

    public FrameBuffer(int width, int height)
    {
        _width = width;
        _height = height;
        _pixels = new byte[width * height];
        Clear(0);
    }

    #endregion

    #region Public Methods

    public void Clear(byte colorIndex) => Array.Fill(_pixels, colorIndex);

    public void DrawRect(int x, int y, int width, int height, byte colorIndex)
    {
        if (!IsValidRect(x, y, width, height, colorIndex))
        {
            Logger.Error($"Invalid DrawRect: x={x}, y={y}, w={width}, h={height}, index={colorIndex}");
            return;
        }

        for (var py = y; py < y + height; py++)
            for (var px = x; px < x + width; px++)
                _pixels[py * _width + px] = colorIndex;
    }

    public void DrawSprite(int x, int y, byte spriteIndex, SpriteRotation rotation = SpriteRotation.None, bool flipX = false, bool flipY = false)
    {
        if (Scene.SpriteSheet == null)
        {
            Logger.Error($"Scene.SpriteSheet is null");
            return;
        }

        var spriteData = Scene.SpriteSheet.GetSpriteData(spriteIndex);
        for (var sy = 0; sy < SpriteSheet.SPRITE_SIZE; sy++)
            for (var sx = 0; sx < SpriteSheet.SPRITE_SIZE; sx++)
            {
                var (srcX, srcY) = GetSourceIndices(sx, sy, rotation, flipX, flipY);
                var index = spriteData[srcY * SpriteSheet.SPRITE_SIZE + srcX];

                if (index == 0) continue;

                var px = x + sx;
                var py = y + (SpriteSheet.SPRITE_SIZE - 1 - sy);

                if (px < 0 || px >= _width || py < 0 || py >= _height)
                    continue;

                _pixels[py * _width + px] = index;
            }
    }

    public byte[] GetPixelData() => _pixels;

    public void DrawToScreen(byte[] indices, uint offset, uint size)
        // TODO: Validate
        => Array.Copy(indices, 0, _pixels, offset, size);

    #endregion

    #region Private Methods

    private (int x, int y) GetSourceIndices(int sx, int sy, SpriteRotation rotation, bool flipX, bool flipY)
    {
        // Apply rotation
        int rotX, rotY;
        switch (rotation)
        {
            case SpriteRotation.Rotate90:
                rotX = sy;
                rotY = SpriteSheet.SPRITE_SIZE - 1 - sx;
                break;
            case SpriteRotation.Rotate180:
                rotX = SpriteSheet.SPRITE_SIZE - 1 - sx;
                rotY = SpriteSheet.SPRITE_SIZE - 1 - sy;
                break;
            case SpriteRotation.Rotate270:
                rotX = SpriteSheet.SPRITE_SIZE - 1 - sy;
                rotY = sx;
                break;
            default: // None
                rotX = sx;
                rotY = sy;
                break;
        }

        // Apply flips
        var srcX = flipX ? SpriteSheet.SPRITE_SIZE - 1 - rotX : rotX;
        var srcY = flipY ? SpriteSheet.SPRITE_SIZE - 1 - rotY : rotY;

        return (srcX, srcY);
    }

    private bool IsValidRect(int x, int y, int width, int height, byte colorIndex)
        => x >= 0 && x + width <= _width && y >= 0 && y + height <= _height && colorIndex <= 16;

    #endregion
}
