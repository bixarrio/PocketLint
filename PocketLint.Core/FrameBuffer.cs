using OpenTK.Mathematics;

namespace PocketLint.Core;

public class FrameBuffer
{
    #region Properties and Fields

    private const int WIDTH = 128;
    private const int HEIGHT = 128;
    private const int SPRITE_SIZE = 8;
    private const int MAX_SPRITES = 256
        ;
    private readonly Color4[] _pixels = new Color4[WIDTH * HEIGHT];
    private readonly byte[,] _spriteSheet = new byte[MAX_SPRITES, SPRITE_SIZE * SPRITE_SIZE];
    private readonly ulong[] _spriteTransparency = new ulong[MAX_SPRITES];
    private readonly Color4[] _palette =
        [
        new(0f, 0f, 0f, 1f),          //  0: Black
        new(0.11f, 0.16f, 0.31f, 1f), //  1: Dark Blue
        new(0.48f, 0.19f, 0.28f, 1f), //  2: Dark Purple
        new(0.31f, 0.37f, 0.24f, 1f), //  3: Dark Green
        new(0.69f, 0.35f, 0.29f, 1f), //  4: Brown
        new(0.38f, 0.35f, 0.35f, 1f), //  5: Dark Gray
        new(0.82f, 0.75f, 0.75f, 1f), //  6: Light Gray
        new(1f, 1f, 1f, 1f),          //  7: White
        new(0.94f, 0.29f, 0.27f, 1f), //  8: Red
        new(0.95f, 0.55f, 0.26f, 1f), //  9: Orange
        new(0.89f, 0.80f, 0.27f, 1f), // 10: Yellow
        new(0.45f, 0.76f, 0.29f, 1f), // 11: Green
        new(0.26f, 0.62f, 0.80f, 1f), // 12: Blue
        new(0.60f, 0.47f, 0.73f, 1f), // 13: Indigo
        new(0.85f, 0.54f, 0.65f, 1f), // 14: Pink
        new(0.95f, 0.75f, 0.55f, 1f)  // 15: Peach
        ];

    public int Width => WIDTH;
    public int Height => HEIGHT;

    #endregion

    #region ctor

    public FrameBuffer()
    {
        Clear(0);
        InitializeSpriteSheet();
    }

    #endregion

    #region Public Methods

    public void SetPixel(int x, int y, int paletteIndex)
    {
        if (x < 0 || x >= WIDTH || y < 0 || y >= HEIGHT) return;
        if (paletteIndex < 0 || paletteIndex >= _palette.Length) return;
        _pixels[y * WIDTH + x] = _palette[paletteIndex];
    }

    public Color4 GetPixel(int x, int y)
    {
        if (x < 0 || x >= WIDTH || y < 0 || y >= HEIGHT) return Color4.Black;
        return _pixels[y * WIDTH + x];
    }

    public virtual void Clear(int paletteIndex)
    {
        if (paletteIndex < 0 || paletteIndex >= _palette.Length) return;
        var color = _palette[paletteIndex];
        for (int i = 0; i < _pixels.Length; i++)
            _pixels[i] = color;
    }

    public virtual void Sprite(int spriteId, int x, int y)
    {
        if (spriteId < 0 || spriteId >= MAX_SPRITES) return;
        var transparencyMask = _spriteTransparency[spriteId];
        for (int sy = 0; sy < SPRITE_SIZE; sy++)
        {
            for (int sx = 0; sx < SPRITE_SIZE; sx++)
            {
                var px = x + sx;
                var py = y + sy;
                if (px < 0 || px >= WIDTH || py < 0 || py >= HEIGHT) continue;
                var index = sy * SPRITE_SIZE + sx;
                if ((transparencyMask & (1ul << index)) != 0) continue;
                var paletteIndex = _spriteSheet[spriteId, sy * SPRITE_SIZE + sx];
                SetPixel(px, py, paletteIndex);
            }
        }
    }

    public void SetSpritePixel(int spriteId, int x, int y, int paletteIndex, bool transparent = false)
    {
        if (spriteId < 0 || spriteId >= MAX_SPRITES) return;
        if (x < 0 || x >= SPRITE_SIZE || y < 0 || y >= SPRITE_SIZE) return;
        if (paletteIndex < 0 || paletteIndex >= _palette.Length) return;
        var index = y * SPRITE_SIZE + x;
        _spriteSheet[spriteId, index] = (byte)paletteIndex;
        if (transparent) _spriteTransparency[spriteId] |= 1ul << index;
        else _spriteTransparency[spriteId] &= ~(1ul << index);
    }

    public Color4[] GetPixels() => _pixels;

    #endregion

    #region Private Methods

    private void InitializeSpriteSheet()
    {
        SetSpritePixel(0, 2, 2, 0);
        SetSpritePixel(0, 2, 3, 0);
        SetSpritePixel(0, 2, 4, 0);
        SetSpritePixel(0, 2, 5, 0);
        SetSpritePixel(0, 5, 2, 0);
        SetSpritePixel(0, 5, 3, 0);
        SetSpritePixel(0, 5, 4, 0);
        SetSpritePixel(0, 5, 5, 0);
        SetSpritePixel(0, 3, 2, 0);
        SetSpritePixel(0, 3, 5, 0);
        SetSpritePixel(0, 4, 2, 0);
        SetSpritePixel(0, 4, 5, 0);
        SetSpritePixel(0, 3, 3, 8);
        SetSpritePixel(0, 3, 4, 8);
        SetSpritePixel(0, 4, 3, 8);
        SetSpritePixel(0, 4, 4, 8);
        SetSpritePixel(0, 0, 0, 0, true);
    }

    #endregion
}
