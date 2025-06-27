using OpenTK.Graphics.OpenGL4;
using PocketLint.Core.Components;
using PocketLint.Core.Logging;
using PocketLint.Core.Rendering;
using PocketLint.Tools;

namespace PocketLint.Runner;
internal class ImageLogger : Renderer, ILogger
{
    #region Properties and Fields

    private int _maxLogLines = -1;
    private List<string> _runningLogLines = new();

    private int _overlayTexture;

    #endregion

    #region Public Methods

    public override void Init()
    {
        _maxLogLines = TextImage.ComputeMaxLogLines(128);
    }

    public unsafe override void Render(FrameBuffer _)
    {
        // This is not happening yet. We need to introduce layers
        // first, and then also a way for me to draw an image directly
        // to a layer instead of using the framebuffer
    }

    public void Log(string message) => AddLogLine($"[LOG] {message}");
    public void Warn(string message) => AddLogLine($"[WARN] {message}");
    public void Error(string message) => AddLogLine($"[ERROR] {message}");

    #endregion

    #region Private Methods

    private void AddLogLine(string message)
    {
        // We will draw from the bottom of the screen upwards. We want the
        // newer entries to be at the bottom and push older text up. To do
        // this properly, we will put newer entries at the top of this list
        _runningLogLines.Insert(0, message);
        // Trim off the trailing (older) bits that exceed the allowed lines
        if (_maxLogLines < 0) _maxLogLines = TextImage.ComputeMaxLogLines(128);
        _runningLogLines = _maxLogLines < 0 ? new() : _runningLogLines[.._maxLogLines];

        UpdateTexture();
    }

    private unsafe void UpdateTexture()
    {
        using var image = TextImage.RenderLogOverlay(_runningLogLines);
        GL.BindTexture(TextureTarget.Texture2D, _overlayTexture);
        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                fixed (void* ptr = &accessor.GetRowSpan(y)[0])
                {
                    GL.TexImage2D(
                        TextureTarget.Texture2D,
                        0,
                        PixelInternalFormat.Rgba,
                        128, 128,
                        0,
                        PixelFormat.Rgba,
                        PixelType.UnsignedByte,
                        (IntPtr)ptr);
                }
            }
        });

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
    }

    #endregion
}
