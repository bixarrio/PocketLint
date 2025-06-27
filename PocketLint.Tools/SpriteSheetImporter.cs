using PocketLint.Core.Entities;
using PocketLint.Core.Rendering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PocketLint.Tools;

public static class SpriteSheetImporter
{
    #region Properties and Fields

    private const int EXPECTED_WIDTH = 128;
    private const int EXPECTED_HEIGHT = 128;

    #endregion

    #region Public Methods

    public static void Import(string path, SpriteSheet target)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("PNG Path cannot be null or empty", nameof(path));

        using var image = Image.Load<Rgba32>(path);
        if (image.Width != EXPECTED_WIDTH || image.Height != EXPECTED_HEIGHT)
            throw new ArgumentException($"PNG must be {EXPECTED_WIDTH}x{EXPECTED_HEIGHT}, got {image.Width}x{image.Height}");

        ImportSpriteSheet(image, target);
    }

    public static void Import(Stream stream, SpriteSheet target)
    {
        using var image = Image.Load<Rgba32>(stream);
        if (image.Width != EXPECTED_WIDTH || image.Height != EXPECTED_HEIGHT)
            throw new ArgumentException($"PNG must be {EXPECTED_WIDTH}x{EXPECTED_HEIGHT}, got {image.Width}x{image.Height}");

        ImportSpriteSheet(image, target);
    }

    #endregion

    #region Private Methods

    private static void ImportSpriteSheet(Image<Rgba32> image, SpriteSheet target)
    {
        var palette = Scene.Palette.GetColorData();

        for (var spriteIndex = 0; spriteIndex < SpriteSheet.SPRITE_COUNT; spriteIndex++)
        {
            var spriteData = ProcessSprite(image, spriteIndex, palette);
            target.SetSpriteData((byte)spriteIndex, spriteData);
        }

        byte[] ProcessSprite(Image<Rgba32> image, int spriteIndex, byte[] palette)
        {
            var spriteData = new byte[SpriteSheet.SPRITE_BYTES];
            var xStart = (spriteIndex % 16) * SpriteSheet.SPRITE_SIZE;
            var yStart = (spriteIndex / 16) * SpriteSheet.SPRITE_SIZE;

            for (var y = 0; y < SpriteSheet.SPRITE_SIZE; y++)
            {
                for (var x = 0; x < SpriteSheet.SPRITE_SIZE; x++)
                {
                    var pixelIndex = y * SpriteSheet.SPRITE_SIZE + x;
                    var pixel = image[xStart + x, yStart + y];
                    spriteData[pixelIndex] = GetPaletteIndex(pixel, palette);
                }
            }
            return spriteData;
        }

        byte GetPaletteIndex(Rgba32 pixel, byte[] palette)
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

        float ColorDistance(byte r1, byte g1, byte b1, byte r2, byte g2, byte b2)
        {
            var dr = r1 - r2;
            var dg = g1 - g2;
            var db = b1 - b2;
            return dr * dr + dg * dg + db * db;
        }
    }

    #endregion
}
