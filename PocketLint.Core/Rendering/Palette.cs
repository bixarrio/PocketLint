using System;

namespace PocketLint.Core.Rendering;
public class Palette
{
    #region Properties and Fields

    public const int COLOR_COUNT = 16;
    public const int BYTES_PER_COLOR = 4;

    private readonly byte[] _colors;

    public bool IsDirty { get; private set; }

    #endregion

    #region ctor

    public Palette()
    {
        _colors = new byte[COLOR_COUNT * BYTES_PER_COLOR]
        {
            // Indices 1–16 (index 0 = transparent, not stored)
              0,   0,   0, 255, //  1: Black
             28,  40,  79, 255, //  2: Dark Blue
            122,  48,  71, 255, //  3: Dark Purple
             79,  94,  61, 255, //  4: Dark Green
            175,  89,  73, 255, //  5: Brown
             96,  89,  89, 255, //  6: Dark Gray
            209, 191, 191, 255, //  7: Light Gray
            255, 255, 255, 255, //  8: White
            239,  73,  68, 255, //  9: Red
            242, 140,  66, 255, // 10: Orange
            226, 204,  68, 255, // 11: Yellow
            114, 193,  73, 255, // 12: Green
             66, 158, 204, 255, // 13: Blue
            153, 119, 186, 255, // 14: Indigo
            216, 137, 165, 255, // 15: Pink
            242, 191, 140, 255  // 16: Peach
        };
        IsDirty = true;
    }

    #endregion

    #region Public Methods

    public byte[] GetColorData()
    {
        IsDirty = false;
        return _colors;
    }

    public void SetColor(int index, byte r, byte g, byte b, byte a = 255)
    {
        if (index < 1 || index > COLOR_COUNT)
            throw new ArgumentException($"Invalid palette index: {index}. Must be 1-{COLOR_COUNT}");
        var offset = (index - 1) * BYTES_PER_COLOR;
        _colors[offset + 0] = r;
        _colors[offset + 1] = g;
        _colors[offset + 2] = b;
        _colors[offset + 3] = a;
        IsDirty = true;
    }
    public (byte r, byte g, byte b, byte a) GetColor(int index)
    {
        if (index < 1 || index > COLOR_COUNT)
            throw new ArgumentException($"Invalid palette index: {index}. Must be 1-{COLOR_COUNT}");
        var offset = (index - 1) * BYTES_PER_COLOR;
        return (
            r: _colors[offset + 0],
            g: _colors[offset + 1],
            b: _colors[offset + 2],
            a: _colors[offset + 3]);
    }

    #endregion
}
