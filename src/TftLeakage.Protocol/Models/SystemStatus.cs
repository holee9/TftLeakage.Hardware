using TftLeakage.Hardware.Models;

namespace TftLeakage.Protocol.Models;

/// <summary>
/// System status information
/// </summary>
public class SystemStatus
{
    /// <summary>Current idle mode state</summary>
    public bool IsIdleMode { get; set; }

    /// <summary>Current bias level</summary>
    public BiasLevel CurrentBias { get; set; }

    /// <summary>Panel temperature in Celsius</summary>
    public float Temperature { get; set; }

    /// <summary>ADC raw value</summary>
    public ushort AdcValue { get; set; }

    /// <summary>Firmware version string</summary>
    public string? FirmwareVersion { get; set; }

    /// <summary>Uptime in seconds</summary>
    public uint UptimeSeconds { get; set; }

    /// <summary>Timestamp of status update</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
