using System;
using System.IO;

namespace PocketLint.Core.Tests
{
    public class LoggerTests
    {
        [Fact]
        public void ConsoleLogger_Log_WritesToConsole()
        {
            // Arrange
            var logger = new ConsoleLogger();
            var message = "Test message";
            var output = new StringWriter();
            Console.SetOut(output);

            // Act
            logger.Log(message);

            // Assert
            Assert.Contains($"[LOG] {message}", output.ToString());
        }

        [Fact]
        public void ConsoleLogger_Warn_WritesToConsole()
        {
            // Arrange
            var logger = new ConsoleLogger();
            var message = "Warning message";
            var output = new StringWriter();
            Console.SetOut(output);

            // Act
            logger.Warn(message);

            // Assert
            Assert.Contains($"[WARN] {message}", output.ToString());
        }

        [Fact]
        public void ConsoleLogger_Error_WritesToConsole()
        {
            // Arrange
            var logger = new ConsoleLogger();
            var message = "Error message";
            var output = new StringWriter();
            Console.SetOut(output);

            // Act
            logger.Error(message);

            // Assert
            Assert.Contains($"[ERROR] {message}", output.ToString());
        }
    }
}
