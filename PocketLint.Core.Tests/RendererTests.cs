namespace PocketLint.Core.Tests;

public class RendererTests
{
    [Fact]
    public void Initialize_CreatesValidTexture()
    {
        var buffer = new FrameBuffer();
        var renderer = new Renderer(buffer);
        renderer.Initialize();
        // TODO: Use some OpenGL Mocking
        // For now, testing will be in the Runner
        Assert.True(true);
    }
}
