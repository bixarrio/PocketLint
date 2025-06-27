using OpenTK.Graphics.OpenGL4;
using PocketLint.Core.Logging;
using System;
using System.IO;
using System.Reflection;

namespace PocketLint.Core.Rendering;

public class ShaderCompileException : Exception
{
    public ShaderCompileException(string message) : base(message) { }
}

public class ShaderLinkException : Exception
{
    public ShaderLinkException(string message) : base(message) { }
}

public class ShaderLoader
{
    #region Properties and Fields

    private const string RESOURCE_NAME_VERTEX_SHADER = "PocketLint.Core.Rendering.vertex.glsl";
    private const string RESOURCE_NAME_FRAGMENT_SHADER = "PocketLint.Core.Rendering.fragment.glsl";

    #endregion

    #region Public Methods

    public int LoadProgram()
    {
        var vertexShaderSource = LoadShaderResource(RESOURCE_NAME_VERTEX_SHADER);
        var fragmentShaderSource = LoadShaderResource(RESOURCE_NAME_FRAGMENT_SHADER);

        var vertexShader = CompileShader(vertexShaderSource, ShaderType.VertexShader);
        var fragmentShader = CompileShader(fragmentShaderSource, ShaderType.FragmentShader);
        var program = LinkProgram(vertexShader, fragmentShader);

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        return program;
    }

    #endregion

    #region Private Methods

    private string LoadShaderResource(string resourceName)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            Logger.Error($"Shader resource not found: {resourceName}");
            var resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            Logger.Log($"Resources available: {string.Join(", ", resources)}");
            return string.Empty;
        }
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private int CompileShader(string source, ShaderType shaderType)
    {
        var shader = GL.CreateShader(shaderType);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

        GL.GetShader(shader, ShaderParameter.CompileStatus, out var status);
        if (status != 1)
        {
            var error = GL.GetShaderInfoLog(shader);
            Logger.Error($"{shaderType} error: {error}");
            throw new ShaderCompileException($"{shaderType} compilation failed: {error}");
        }

        return shader;
    }

    private int LinkProgram(int vertexShader, int fragmentShader)
    {
        var program = GL.CreateProgram();
        GL.AttachShader(program, vertexShader);
        GL.AttachShader(program, fragmentShader);
        GL.LinkProgram(program);

        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var status);
        if (status != 1)
        {
            var error = GL.GetProgramInfoLog(program);
            Logger.Error($"Program link error: {error}");
            throw new ShaderLinkException($"Program linking failed: {error}");
        }

        return program;
    }

    #endregion
}
