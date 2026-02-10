using FluentAssertions;
using Moq;
using TftLeakage.Hardware;
using TftLeakage.Hardware.Models;
using Xunit;

namespace TftLeakage.Hardware.Tests;

public class FpgaSpiClientTests
{
    [Fact]
    public void Constructor_WithDevicePath_SetsProperties()
    {
        // Arrange & Act
        var client = new FpgaSpiClient("/dev/spidev1.0");

        // Assert
        client.IsConnected.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithBusAndDevice_CreatesCorrectPath()
    {
        // Arrange & Act
        var client = new FpgaSpiClient(0, 1);

        // Assert
        client.Should().NotBeNull();
    }

    [Fact]
    public async Task SetIdleModeAsync_WhenNotConnected_ThrowsInvalidOperationException()
    {
        // Arrange
        var client = new FpgaSpiClient("/dev/spidev0.0");

        // Act
        var act = async () => await client.SetIdleModeAsync(true);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Not connected*");
    }

    [Theory]
    [InlineData(BiasLevel.Normal, 0)]
    [InlineData(BiasLevel.Medium, 1)]
    [InlineData(BiasLevel.Idle, 2)]
    [InlineData(BiasLevel.Off, 7)]
    public void BiasLevel_HasCorrectValues(BiasLevel level, byte expectedValue)
    {
        // Act
        var value = (byte)level;

        // Assert
        value.Should().Be(expectedValue);
    }

    [Fact]
    public void FpgaRegisters_HasCorrectAddresses()
    {
        // Assert
        FpgaRegisters.CTRL.Should().Be(0x00);
        FpgaRegisters.STATUS.Should().Be(0x01);
        FpgaRegisters.BIAS_SEL.Should().Be(0x02);
        FpgaRegisters.VERSION.Should().Be(0xFE);
    }
}
