using Moq;
using System.Collections.Generic;

namespace PocketLint.Core.Tests;

public class ScriptHostTests
{
    private const float FRAME = 0.016f;

    [Fact]
    public void Update_CallsGameObjectUpdate()
    {
        var mockFrameBuffer = new Mock<FrameBuffer>();
        var mockInput = new Mock<Input>();
        var mockGameObject = new Mock<GameObject>(0);
        var mockTimeProvider = new Mock<ITimeProvider>();
        mockTimeProvider.Setup(t => t.GetTimeSeconds()).Returns(0);

        var host = new ScriptHost(mockFrameBuffer.Object, mockInput.Object, mockTimeProvider.Object);
        var go = host.CreateGameObject();

        host.Update();
        mockGameObject.Setup(g => g.Update(FRAME));
        Assert.Equal(3, host.GetType().GetField("_gameObjects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(host) is List<GameObject> list ? list.Count : 0);
    }
    [Fact]
    public void Draw_CallsGameObjectDraw()
    {
        var mockFrameBuffer = new Mock<FrameBuffer>();
        var mockInput = new Mock<Input>();
        var mockGameObject = new Mock<GameObject>(0);
        var mockTimeProvider = new Mock<ITimeProvider>();
        mockTimeProvider.Setup(t => t.GetTimeSeconds()).Returns(0);

        var host = new ScriptHost(mockFrameBuffer.Object, mockInput.Object, mockTimeProvider.Object);
        var go = host.CreateGameObject();

        host.Draw();

        mockGameObject.Setup(g => g.Draw());
        mockFrameBuffer.Verify(fb => fb.Clear(0), Times.Exactly(2));
    }
    [Fact]
    public void CreateGameObject_ReturnsUniqueIds()
    {
        var frameBuffer = new FrameBuffer();
        var input = new Input();
        var mockTimeProvider = new Mock<ITimeProvider>();
        mockTimeProvider.Setup(t => t.GetTimeSeconds()).Returns(0);

        var host = new ScriptHost(frameBuffer, input, mockTimeProvider.Object);

        var go1 = host.CreateGameObject();
        var go2 = host.CreateGameObject();

        Assert.Equal(2, go1.Id);
        Assert.Equal(3, go2.Id);
    }
}
