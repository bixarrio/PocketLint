using Moq;

namespace PocketLint.Core.Tests;

public class ComponentSystemTests
{
    private const float FRAME = 0.016f;

    [Fact]
    public void GameObject_Update_CallsComponentUpdate()
    {
        var go = new GameObject(1);
        var mockComponent = new Mock<IComponent>();
        mockComponent.SetupGet(c => c.GameObject).Returns(go);
        go.AddComponent(mockComponent.Object);

        go.Update(FRAME);

        mockComponent.Verify(c => c.Update(FRAME), Times.Once());
    }

    [Fact]
    public void GameObject_Draw_CallsComponentDraw()
    {
        var go = new GameObject(1);
        var mockComponent = new Mock<IComponent>();
        mockComponent.SetupGet(c => c.GameObject).Returns(go);
        go.AddComponent(mockComponent.Object);

        go.Draw();

        mockComponent.Verify(c => c.Draw(), Times.Once());
    }

    [Fact]
    public void GameObject_GetComponent_ReturnsCorrectComponent()
    {
        var go = new GameObject(1);
        var transform = new Transform(go);
        go.AddComponent(transform);

        var result = go.GetComponent<Transform>();

        Assert.Same(transform, result);
    }

    [Fact]
    public void SpriteRenderer_Draw_AppliesCameraOffset()
    {
        var mockFrameBuffer = new Mock<FrameBuffer>();
        var mockInput = new Mock<Input>();
        var mockTimeProvider = new Mock<ITimeProvider>();
        mockTimeProvider.Setup(t => t.GetTimeSeconds()).Returns(0);
        var host = new ScriptHost(mockFrameBuffer.Object, mockInput.Object, mockTimeProvider.Object);
        var playerGo = host.CreateGameObject();
        var transform = new Transform(playerGo, 50f, 64f);
        var spriteRenderer = new SpriteRenderer(mockFrameBuffer.Object, playerGo, 0);
        playerGo.AddComponent(transform);
        playerGo.AddComponent(spriteRenderer);

        host.Draw();

        mockFrameBuffer.Verify(fb => fb.Sprite(0, 18, 32), Times.Once());
    }

    [Fact]
    public void PlayerController_Update_MovesTransformWithDeltaTime()
    {
        var mockInput = new Mock<Input>();
        mockInput.Setup(i => i.Button(1)).Returns(true);

        var playerGo = new GameObject(1);

        var transform = new Transform(playerGo, 10f, 20f);
        var playerController = new PlayerController(mockInput.Object, playerGo);
        playerGo.AddComponent(transform);
        playerGo.AddComponent(playerController);

        playerGo.Update(FRAME);

        Assert.Equal(10.512f, transform.X, 3);
        Assert.Equal(20f, transform.Y, 3);
    }

    [Fact]
    public void ScriptComponent_Update_ExecutesAction()
    {
        var wasCalled = false;
        var go = new GameObject(1);
        var script = new ScriptComponent(go, updateAction: (_) => wasCalled = true);
        go.AddComponent(script);
        go.Update(FRAME);
        Assert.True(wasCalled);
    }

    [Fact]
    public void ScriptComponent_Draw_ExecutesAction()
    {
        var wasCalled = false;
        var go = new GameObject(1);
        var script = new ScriptComponent(go, drawAction: () => wasCalled = true);
        go.AddComponent(script);
        go.Draw();
        Assert.True(wasCalled);
    }
}
