namespace TftLeakage.Hardware.Models;

/// <summary>
/// Bias voltage level selection
/// </summary>
public enum BiasLevel : byte
{
    /// <summary>Normal mode bias voltage (V1)</summary>
    Normal = 0b000,

    /// <summary>Medium bias voltage (V2)</summary>
    Medium = 0b001,

    /// <summary>Idle mode bias voltage (V3)</summary>
    Idle = 0b010,

    /// <summary>Bias voltage disabled (OFF)</summary>
    Off = 0b111
}
