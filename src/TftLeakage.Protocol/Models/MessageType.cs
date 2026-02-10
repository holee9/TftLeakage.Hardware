namespace TftLeakage.Protocol.Models;

/// <summary>
/// Message type identifiers for TFT protocol
/// </summary>
public enum MessageType : byte
{
    /// <summary>Status request from host</summary>
    StatusRequest = 0x01,

    /// <summary>Status response from device</summary>
    StatusResponse = 0x02,

    /// <summary>Set idle mode command</summary>
    SetIdleMode = 0x10,

    /// <summary>Set bias level command</summary>
    SetBiasLevel = 0x11,

    /// <summary>Event notification from device</summary>
    EventNotification = 0x20,

    /// <summary>Error response</summary>
    ErrorResponse = 0xFF
}
