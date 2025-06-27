using PocketLint.Core.Logging;
using System;

namespace PocketLint.Core.Rendering;
public class SpriteSheet
{
    #region Properties and Fields

    public const int SPRITE_SIZE = 8;
    public const int SPRITE_COUNT = 256;
    public const int SPRITE_BYTES = SPRITE_SIZE * SPRITE_SIZE;

    private readonly byte[,] _sprites;

    #endregion

    #region ctor

    public SpriteSheet()
    {
        _sprites = new byte[SPRITE_COUNT, SPRITE_BYTES];
        FillDefaultSpriteSheet();
    }

    #endregion

    #region Public Methods

    public byte[] GetSpriteData(byte spriteIndex)
    {
        if (spriteIndex < 0 || spriteIndex > SPRITE_COUNT - 1)
        {
            Logger.Error($"Invalid sprite index: {spriteIndex}. Must be 0-{SPRITE_COUNT - 1}");
            throw new ArgumentException($"Invalid sprite index: {spriteIndex}. Must be 0-{SPRITE_COUNT - 1}");
        }
        var data = new byte[SPRITE_BYTES];
        for (var i = 0; i < SPRITE_BYTES; i++)
            data[i] = _sprites[spriteIndex, i];
        return data;
    }

    public void SetSpriteData(byte spriteIndex, byte[] data)
    {
        if (spriteIndex < 0 || spriteIndex > SPRITE_COUNT - 1)
        {
            Logger.Error($"Invalid sprite index: {spriteIndex}. Must be 0-{SPRITE_COUNT - 1}");
            throw new ArgumentException($"Invalid sprite index: {spriteIndex}. Must be 0-{SPRITE_COUNT - 1}");
        }
        if (data == null || data.Length != SPRITE_BYTES)
        {
            Logger.Error($"Invalid sprite data length: {data?.Length ?? 0}. Must be {SPRITE_BYTES}");
            throw new ArgumentException($"Invalid sprite data length: {data?.Length ?? 0}. Must be {SPRITE_BYTES}");
        }
        for (var i = 0; i < SPRITE_BYTES; i++)
            _sprites[spriteIndex, i] = data[i];
        Logger.Log($"Set sprite data for index {spriteIndex}");
    }

    #endregion

    #region Private Methods

    private void FillDefaultSpriteSheet()
    {
        Logger.Log($"Filling default spritesheet");

        // Just a spritesheet filled with checkered patterns
        for (int i = 0; i < 256; i++)
        {
            byte n = 15; // (byte)(2 + (i % 15));

            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                    _sprites[i, y * 8 + x] = 1;

            for (int y = 0; y < 4; y++)
                for (int x = 4; x < 8; x++)
                    _sprites[i, y * 8 + x] = n;

            for (int y = 4; y < 8; y++)
                for (int x = 0; x < 4; x++)
                    _sprites[i, y * 8 + x] = n;

            for (int y = 4; y < 8; y++)
                for (int x = 4; x < 8; x++)
                    _sprites[i, y * 8 + x] = 1;
        }
    }

    #endregion
}
