using TftLeakage.Protocol.Models;

namespace TftLeakage.Protocol.Messages;

/// <summary>
/// Status response message payload
/// </summary>
public class StatusResponse
{
    /// <summary>Status flags (bit 0: idle mode)</summary>
    public byte Flags { get; set; }

    /// <summary>Current bias level</summary>
    public byte BiasLevel { get; set; }

    /// <summary>Temperature in 0.1°C units</summary>
    public ushort Temperature { get; set; }

    /// <summary>ADC raw value</summary>
    public ushort AdcValue { get; set; }

    /// <summary>Uptime in seconds</summary>
    public uint UptimeSeconds { get; set; }

    /// <summary>Serialize to bytes</summary>
    public byte[] ToBytes()
    {
        var buffer = new byte[10];
        buffer[0] = Flags;
        buffer[1] = BiasLevel;
        buffer[2] = (byte)((Temperature >> 8) & 0xFF);
        buffer[3] = (byte)(Temperature & 0xFF);
        buffer[4] = (byte)((AdcValue >> 8) & 0xFF);
        buffer[5] = (byte)(AdcValue & 0xFF);
        buffer[6] = (byte)((UptimeSeconds >> 24) & 0xFF);
        buffer[7] = (byte)((UptimeSeconds >> 16) & 0xFF);
        buffer[8] = (byte)((UptimeSeconds >> 8) & 0xFF);
        buffer[9] = (byte)(UptimeSeconds & 0xFF);
        return buffer;
    }

    /// <summary>Parse from bytes</summary>
    public static StatusResponse FromBytes(byte[] buffer)
    {
        if (buffer.Length < 10)
            throw new ArgumentException("Buffer too small for status response");

        return new StatusResponse
        {
            Flags = buffer[0],
            BiasLevel = buffer[1],
            Temperature = (ushort)((buffer[2] << 8) | buffer[3]),
            AdcValue = (ushort)((buffer[4] << 8) | buffer[5]),
            UptimeSeconds = (uint)((buffer[6] << 24) | (buffer[7] << 16) | (buffer[8] << 8) | buffer[9])
        };
    }

    /// <summary>Convert to SystemStatus</summary>
    public SystemStatus ToSystemStatus()
    {
        return new SystemStatus
        {
            IsIdleMode = (Flags & 0x01) != 0,
            CurrentBias = (Hardware.Models.BiasLevel)(BiasLevel & 0x07),
            Temperature = Temperature / 10.0f,
            AdcValue = AdcValue,
            UptimeSeconds = UptimeSeconds,
            Timestamp = DateTime.UtcNow
        };
    }
}
