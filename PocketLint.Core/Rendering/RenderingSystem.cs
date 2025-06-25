using OpenTK.Graphics.OpenGL4;
using PocketLint.Core.Components;
using PocketLint.Core.Entities;
using PocketLint.Core.Logging;
using System;
using System.Linq;

namespace PocketLint.Core.Rendering;

public class RenderingSystem : IDisposable
{
    #region Properties and Fields

    private readonly ShaderLoader _shaderLoader;
    private readonly TextureManager _textureManager;

    private int _shaderProgram;
    private int _vao;
    private bool _isInitialized;
    private bool _disposedValue;

    #endregion

    #region ctor

    public RenderingSystem(ShaderLoader shaderLoader, TextureManager textureManager)
    {
        _shaderLoader = shaderLoader;
        _textureManager = textureManager;
    }

    ~RenderingSystem() => Dispose(false);

    #endregion

    #region Public Methods

    public void Initialize()
    {
        if (_isInitialized)
        {
            Logger.Error("RenderingSystem already initialized");
            return;
        }

        _shaderProgram = _shaderLoader.LoadProgram();
        _vao = CreateRenderQuad();
        _textureManager.Initialize();
        _isInitialized = true;
        Logger.Log("RenderingSystem initialized");
    }

    public void Render(FrameBuffer frameBuffer, Scene scene)
    {
        if (!_isInitialized)
        {
            Logger.Error("RenderingSystem not initialized");
            return;
        }

        frameBuffer.Clear(0);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        var renderers = scene.EntityManager.GetAllComponents<Renderer>()
            .OrderBy(r => r.Component.Layer)
            .ThenBy(r => r.Component.SortOrder);

        foreach (var (_, renderer) in renderers)
            renderer.Render(frameBuffer);

        if (Scene.Palette.IsDirty) _textureManager.UpdatePaletteTexture();
        _textureManager.UpdateIndexTexture(frameBuffer.GetPixelData());
        RenderQuad();
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Protected Methods

    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue) return;
        if (!disposing) return;

        GL.DeleteProgram(_shaderProgram);
        GL.DeleteVertexArray(_vao);
        _textureManager.Dispose();

        _disposedValue = true;
    }

    #endregion

    #region Private Methods

    private int CreateRenderQuad()
    {
        var vertices = new float[] // (x, y), (u, v)
        {
            -1f, -1f, 0f, 0f, // bottom left
             1f, -1f, 1f, 0f, // bottom right
             1f,  1f, 1f, 1f, // top right
            -1f,  1f, 0f, 1f, // top left
        };
        var indices = new int[] { 0, 1, 2, 0, 2, 3 };

        var vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        var vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        var ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);

        return vao;
    }

    private void RenderQuad()
    {
        const string INDEX_TEXTURE_UNIFORM = "indexTexture";
        const string PALETTE_TEXTURE_UNIFORM = "paletteTexture";

        GL.UseProgram(_shaderProgram);

        _textureManager.BindTextures();

        GL.Uniform1(GL.GetUniformLocation(_shaderProgram, INDEX_TEXTURE_UNIFORM), 0);
        GL.Uniform1(GL.GetUniformLocation(_shaderProgram, PALETTE_TEXTURE_UNIFORM), 1);
        GL.BindVertexArray(_vao);
        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
    }

    #endregion
}
