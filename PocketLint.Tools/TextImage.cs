using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PocketLint.Tools;
public static class TextImage
{
    #region Properties and Fields

    private static Font _font;

    #endregion

    #region Public Methods

    public static int ComputeMaxLogLines(int targetHeight)
    {
        var lineHeight = TextMeasurer.MeasureSize("A", new TextOptions(GetFont())).Height;
        return (int)(targetHeight / lineHeight);
    }

    public static Image<Rgba32> RenderLogOverlay(List<string> lines, int width = 128, int height = 128)
    {
        var destImage = new Image<Rgba32>(width, height);

        var options = new RichTextOptions(GetFont())
        {
            Origin = new Point(0, height),
            VerticalAlignment = VerticalAlignment.Bottom
        };

        var yPos = height;
        var lineHeight = TextMeasurer.MeasureSize("A", options).Height;

        for (int i = lines.Count - 1; i >= 0; i--)
        {
            if (yPos - lineHeight < 0f) break;
            options.Origin = new PointF(0, yPos);
            destImage.Mutate(ctx => ctx.DrawText(options, lines[i], Color.White));
            yPos -= (int)(lineHeight + 0.5f); //ceiling
        }
        return destImage;
    }

    #endregion

    #region Private Methods

    private static Font GetFont()
    {
        if (_font == null)
            _font = SystemFonts.CreateFont("Consolas", 12);
        return _font;
    }

    #endregion
}
