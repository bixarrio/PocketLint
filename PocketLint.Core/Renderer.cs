using OpenTK.Graphics.OpenGL4;
using System;

namespace PocketLint.Core;

public class Renderer
{
    #region Properties and Fields

    private readonly FrameBuffer _buffer;
    private int _textureId;
    private int _vao;
    private int _vbo;
    private int _shaderProgram;
    private readonly float[] _quadVertices = new float[] {
        // Position (x, y, z) | TexCoord (u, v)
        -1f, -1f, 0f, 0f, 0f, // Bottom-left
         1f, -1f, 0f, 1f, 0f, // Bottom-right
         1f,  1f, 0f, 1f, 1f, // Top-right
        -1f, -1f, 0f, 0f, 0f, // Bottom-left (triangle 1)
         1f,  1f, 0f, 1f, 1f, // Top-right (triangle 2)
        -1f,  1f, 0f, 0f, 1f // Top-left (triangle 2)
    };

    #endregion

    #region ctor

    public Renderer(FrameBuffer frameBuffer) => _buffer = frameBuffer;

    #endregion

    #region Public Methods

    public void Initialize()
    {
        InitializeTexture();
        InitializeQuad();
        InitializeShader();
    }

    public void Render()
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        CheckGLError("After Clear");

        GL.BindTexture(TextureTarget.Texture2D, _textureId);
        CheckGLError("After BindTexture");

        GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, _buffer.Width, _buffer.Height, PixelFormat.Rgba, PixelType.Float, _buffer.GetPixels());
        CheckGLError("After TexSubImage2D");

        GL.UseProgram(_shaderProgram);
        CheckGLError("After UseProgram");

        GL.BindVertexArray(_vao);
        CheckGLError("After BindVertexArray");

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        CheckGLError("After DrawArrays");
    }

    #endregion

    #region Private Methods

    private void InitializeTexture()
    {
        _textureId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _textureId);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _buffer.Width, _buffer.Height, 0, PixelFormat.Rgba, PixelType.Float, _buffer.GetPixels());
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
    }

    private void InitializeQuad()
    {
        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _quadVertices.Length * sizeof(float), _quadVertices, BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (IntPtr)0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (IntPtr)(3 * sizeof(float)));

        GL.BindVertexArray(0);
    }

    private void InitializeShader()
    {
        const string vertexShaderSource = @"
#version 330 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
out vec2 vTexCoord;
void main()
{
    gl_Position = vec4(aPosition, 1.0);
    vTexCoord = aTexCoord;
}";

        const string fragmentShaderSource = @"
#version 330 core
in vec2 vTexCoord;
uniform sampler2D tex;
out vec4 FragColor;
void main()
{
    FragColor = texture(tex, vTexCoord);
}";

        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexShaderSource);
        GL.CompileShader(vertexShader);
        CheckShaderError(vertexShader, "Vertex Shader");

        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentShaderSource);
        GL.CompileShader(fragmentShader);
        CheckShaderError(fragmentShader, "Fragment Shader");

        _shaderProgram = GL.CreateProgram();
        GL.AttachShader(_shaderProgram, vertexShader);
        GL.AttachShader(_shaderProgram, fragmentShader);
        GL.LinkProgram(_shaderProgram);
        CheckProgramError(_shaderProgram);

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        void CheckShaderError(int shader, string name)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
            if (status == 0)
                throw new Exception($"{name} compilation failed: {GL.GetShaderInfoLog(shader)}");
        }

        void CheckProgramError(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int status);
            if (status == 0)
                throw new Exception($"Program linking failed: {GL.GetProgramInfoLog(program)}");
        }
    }

    private void CheckGLError(string context)
    {
        var error = GL.GetError();
        if (error != ErrorCode.NoError)
            Console.WriteLine($"OpenGL Error in {context}: {error}");
    }

    #endregion
}
