namespace TftLeakage.Hardware.Models;

/// <summary>
/// FPGA register address map
/// </summary>
public static class FpgaRegisters
{
    /// <summary>Control register (RW)</summary>
    public const byte CTRL = 0x00;

    /// <summary>Status register (R)</summary>
    public const byte STATUS = 0x01;

    /// <summary>Bias selection register (RW)</summary>
    public const byte BIAS_SEL = 0x02;

    /// <summary>Scan configuration register (RW)</summary>
    public const byte SCAN_CONFIG = 0x04;

    /// <summary>Timer period low byte (RW)</summary>
    public const byte TIMER_L = 0x10;

    /// <summary>Timer period high byte (RW)</summary>
    public const byte TIMER_H = 0x11;

    /// <summary>ADC data register (R)</summary>
    public const byte ADC_DATA = 0x20;

    /// <summary>Firmware version register (R)</summary>
    public const byte VERSION = 0xFE;
}

/// <summary>
/// Control register bit definitions
/// </summary>
public static class CtrlBits
{
    /// <summary>Idle mode enable bit (bit 0)</summary>
    public const uint IDLE_EN = 1 << 0;
}

/// <summary>
/// Status register bit definitions
/// </summary>
public static class StatusBits
{
    /// <summary>Ready bit (bit 0)</summary>
    public const uint READY = 1 << 0;

    /// <summary>Idle mode status bit (bit 1)</summary>
    public const uint IDLE = 1 << 1;
}
