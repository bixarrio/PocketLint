using Moq;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

namespace PocketLint.Core.Tests;

public class InputTests
{
    [Fact]
    public void Update_SetsButtonStatesCorrectly()
    {
        var mockKeyboard = new Mock<IKeyboardStateProvider>();
        mockKeyboard.Setup(k => k.IsKeyDown(Keys.Left)).Returns(true)
            .Callback(() => Console.WriteLine("Mock: Left key pressed"));
        mockKeyboard.Setup(k => k.IsKeyDown(Keys.Right)).Returns(false)
            .Callback(() => Console.WriteLine("Mock: Right key pressed"));
        mockKeyboard.Setup(k => k.IsKeyDown(Keys.Up)).Returns(false)
            .Callback(() => Console.WriteLine("Mock: Up key pressed"));
        mockKeyboard.Setup(k => k.IsKeyDown(Keys.Down)).Returns(false)
            .Callback(() => Console.WriteLine("Mock: Down key pressed"));
        mockKeyboard.Setup(k => k.IsKeyDown(Keys.Z)).Returns(true)
            .Callback(() => Console.WriteLine("Mock: Z key pressed"));
        mockKeyboard.Setup(k => k.IsKeyDown(Keys.X)).Returns(false)
            .Callback(() => Console.WriteLine("Mock: X key pressed"));

        var input = new Input();
        input.Update(mockKeyboard.Object);

        Assert.True(input.Button(0));
        Assert.False(input.Button(1));
        Assert.True(input.Button(4));
        Assert.False(input.Button(5));
    }

    [Fact]
    public void Button_InvalidIndex_ReturnsFalse()
    {
        var input = new Input();
        Assert.False(input.Button(-1));
        Assert.False(input.Button(6));
    }
}
