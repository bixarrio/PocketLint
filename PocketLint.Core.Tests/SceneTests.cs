using Moq;
using PocketLint.Core.Components;
using PocketLint.Core.Logging;
using System;

namespace PocketLint.Core.Tests;
public class SceneTests : IDisposable
{
    private readonly Mock<ILogger> _mockLogger;

    public SceneTests()
    {
        _mockLogger = new Mock<ILogger>();
        Logger.Register(_mockLogger.Object);
    }

    public void Dispose()
    {
        Logger.Unregister(_mockLogger.Object);
    }

    [Fact]
    public void Scene_CreateEntity_CreateEntityInEntityManager()
    {
        var scene = new Scene("TestScene");

        var entityId = scene.CreateEntity("TestEntity");

        Assert.Equal(1u, entityId);
        _mockLogger.Verify(l => l.Log("Created entity 'TestEntity' with ID 1 at (0, 0)"), Times.Once());
    }

    [Fact]
    public void Scene_CreateEntity_AddsTransform()
    {
        var mockLogger = new Mock<ILogger>();
        Logger.Register(mockLogger.Object);
        var scene = new Scene("TestScene");

        uint entityId = scene.CreateEntity("TestEntity", 10f, 20f);
        var transform = scene.GetComponent<EntityTransform>(entityId);

        Assert.NotNull(transform);
        Assert.Equal(10f, transform.LocalX);
        Assert.Equal(20f, transform.LocalY);
        mockLogger.Verify(l => l.Log("Created entity 'TestEntity' with ID 1 at (10, 20)"), Times.Once());
        Logger.Unregister(mockLogger.Object);
    }
}
