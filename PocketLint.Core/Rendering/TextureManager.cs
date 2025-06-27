using OpenTK.Graphics.OpenGL4;
using PocketLint.Core.Entities;
using PocketLint.Core.Logging;
using System;

namespace PocketLint.Core.Rendering;

public class TextureManager : IDisposable
{
    #region Properties and Fields

    private readonly int _width;
    private readonly int _height;

    private int _indexTexture;
    private int _paletteTexture;
    private bool _disposedValue;

    #endregion

    #region ctor

    public TextureManager(int width, int height)
    {
        _width = width;
        _height = height;
    }

    ~TextureManager() => Dispose(false);

    #endregion

    #region Public Methods

    public void Initialize()
    {
        CreateIndexTexture();
        CreatePaletteTexture();
    }

    public void UpdateIndexTexture(byte[] pixels)
    {
        if (pixels.Length != _width * _height)
        {
            Logger.Error($"Invalid pixel data size: {pixels.Length}, expected {_width * _height}");
            return;
        }
        GL.BindTexture(TextureTarget.Texture2D, _indexTexture);
        GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, _width, _height, PixelFormat.RedInteger, PixelType.UnsignedByte, pixels);
    }

    public void UpdatePaletteTexture()
    {
        GL.BindTexture(TextureTarget.Texture2D, _paletteTexture);
        GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, 16, 1, PixelFormat.Rgba, PixelType.UnsignedByte, Scene.Palette.GetColorData());
    }

    public void BindTextures()
    {
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _indexTexture);
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D, _paletteTexture);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Protected Methods

    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue) return;
        if (!disposing) return;

        GL.DeleteTexture(_indexTexture);
        GL.DeleteTexture(_paletteTexture);

        _disposedValue = true;
    }

    #endregion

    #region Private Methods

    private void CreateIndexTexture()
    {
        _indexTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _indexTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8ui, _width, _height, 0, PixelFormat.RedInteger, PixelType.UnsignedByte, nint.Zero);
        GL.TextureParameter(_indexTexture, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TextureParameter(_indexTexture, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
    }

    private void CreatePaletteTexture()
    {
        _paletteTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _paletteTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 16, 1, 0, PixelFormat.Rgba, PixelType.UnsignedByte, Scene.Palette.GetColorData());
        GL.TextureParameter(_paletteTexture, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TextureParameter(_paletteTexture, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
    }

    #endregion
}
