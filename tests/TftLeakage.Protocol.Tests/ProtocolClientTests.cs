using FluentAssertions;
using TftLeakage.Protocol;
using TftLeakage.Protocol.Messages;
using TftLeakage.Protocol.Models;
using Xunit;

namespace TftLeakage.Protocol.Tests;

public class ProtocolClientTests
{
    [Fact]
    public void MessageHeader_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var header = new MessageHeader();

        // Assert
        header.Magic.Should().Be(0x54465400);
        header.Type.Should().Be(0);
        header.Length.Should().Be(0);
        header.Sequence.Should().Be(0);
    }

    [Fact]
    public void MessageHeader_ToBytes_And_FromBytes_RoundTrip()
    {
        // Arrange
        var original = new MessageHeader
        {
            Type = 0x10,
            Length = 100,
            Sequence = 5
        };

        // Act
        var bytes = original.ToBytes();
        var parsed = MessageHeader.FromBytes(bytes);

        // Assert
        parsed.Magic.Should().Be(original.Magic);
        parsed.Type.Should().Be(original.Type);
        parsed.Length.Should().Be(original.Length);
        parsed.Sequence.Should().Be(original.Sequence);
    }

    [Fact]
    public void MessageHeader_FromBytes_WithInvalidMagic_ThrowsInvalidDataException()
    {
        // Arrange
        var bytes = new byte[8];
        bytes[0] = 0xFF; // Invalid magic

        // Act
        var act = () => MessageHeader.FromBytes(bytes);

        // Assert
        act.Should().Throw<InvalidDataException>()
            .WithMessage("*Invalid magic number*");
    }

    [Fact]
    public void StatusResponse_ToBytes_And_FromBytes_RoundTrip()
    {
        // Arrange
        var original = new StatusResponse
        {
            Flags = 0x01,
            BiasLevel = 2,
            Temperature = 250, // 25.0°C
            AdcValue = 1234,
            UptimeSeconds = 3600
        };

        // Act
        var bytes = original.ToBytes();
        var parsed = StatusResponse.FromBytes(bytes);

        // Assert
        parsed.Flags.Should().Be(original.Flags);
        parsed.BiasLevel.Should().Be(original.BiasLevel);
        parsed.Temperature.Should().Be(original.Temperature);
        parsed.AdcValue.Should().Be(original.AdcValue);
        parsed.UptimeSeconds.Should().Be(original.UptimeSeconds);
    }

    [Fact]
    public void StatusResponse_ToSystemStatus_ConvertsCorrectly()
    {
        // Arrange
        var response = new StatusResponse
        {
            Flags = 0x01,
            BiasLevel = 2,
            Temperature = 250, // 25.0°C
            AdcValue = 1234,
            UptimeSeconds = 3600
        };

        // Act
        var status = response.ToSystemStatus();

        // Assert
        status.IsIdleMode.Should().BeTrue();
        status.CurrentBias.Should().Be(Hardware.Models.BiasLevel.Idle);
        status.Temperature.Should().Be(25.0f);
        status.AdcValue.Should().Be(1234);
        status.UptimeSeconds.Should().Be(3600);
    }

    [Fact]
    public void MessageType_HasCorrectValues()
    {
        // Assert
        ((byte)MessageType.StatusRequest).Should().Be(0x01);
        ((byte)MessageType.StatusResponse).Should().Be(0x02);
        ((byte)MessageType.SetIdleMode).Should().Be(0x10);
        ((byte)MessageType.SetBiasLevel).Should().Be(0x11);
        ((byte)MessageType.EventNotification).Should().Be(0x20);
        ((byte)MessageType.ErrorResponse).Should().Be(0xFF);
    }
}
