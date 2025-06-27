using PocketLint.Core.Entities;
using PocketLint.Core.Rendering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PocketLint.Tools;
public static class ImageImporter
{
    #region Properties and Fields

    private const int EXPECTED_WIDTH = 128;
    private const int EXPECTED_HEIGHT = 128;

    #endregion

    #region Public Methods

    public static byte[] Import(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("PNG Path cannot be null or empty", nameof(path));

        using var image = Image.Load<Rgba32>(path);
        if (image.Width != EXPECTED_WIDTH || image.Height != EXPECTED_HEIGHT)
            throw new ArgumentException($"PNG must be {EXPECTED_WIDTH}x{EXPECTED_HEIGHT}, got {image.Width}x{image.Height}");

        var palette = Scene.Palette.GetColorData();

        return ProcessImage(image, palette);
    }
    public static byte[] ImportStream(Stream stream, bool flipX = false, bool flipY = false)
    {
        using var image = Image.Load<Rgba32>(stream);
        if (image.Width != EXPECTED_WIDTH || image.Height != EXPECTED_HEIGHT)
            throw new ArgumentException($"PNG must be {EXPECTED_WIDTH}x{EXPECTED_HEIGHT}, got {image.Width}x{image.Height}");

        var palette = Scene.Palette.GetColorData();

        return ProcessImage(image, palette, flipX, flipY);
    }
    private static byte[] ProcessImage(Image<Rgba32> image, byte[] palette, bool flipX = false, bool flipY = false)
    {
        var spriteData = new byte[EXPECTED_WIDTH * EXPECTED_HEIGHT];
        for (var y = 0; y < EXPECTED_HEIGHT; y++)
        {
            for (var x = 0; x < EXPECTED_WIDTH; x++)
            {
                var px = flipX ? EXPECTED_WIDTH - 1 - x : x;
                var py = flipY ? EXPECTED_HEIGHT - 1 - y : y;

                var pixelIndex = y * EXPECTED_WIDTH + x;
                var pixel = image[px, py];
                spriteData[pixelIndex] = GetPaletteIndex(pixel, palette);
            }
        }
        return spriteData;
    }
    private static byte GetPaletteIndex(Rgba32 pixel, byte[] palette)
    {
        if (pixel.A == 0) return 0;

        var minDistance = float.MaxValue;
        var bestIndex = 1;

        for (var i = 0; i < Palette.COLOR_COUNT; i++)
        {
            var offset = i * Palette.BYTES_PER_COLOR;
            var distance = ColorDistance(
                pixel.R, pixel.G, pixel.B,
                palette[offset], palette[offset + 1], palette[offset + 2]);
            if (distance < minDistance)
            {
                minDistance = distance;
                bestIndex = i + 1;
            }
        }
        return (byte)bestIndex;
    }

    private static float ColorDistance(byte r1, byte g1, byte b1, byte r2, byte g2, byte b2)
    {
        var dr = r1 - r2;
        var dg = g1 - g2;
        var db = b1 - b2;
        return dr * dr + dg * dg + db * db;
    }

    #endregion
}
