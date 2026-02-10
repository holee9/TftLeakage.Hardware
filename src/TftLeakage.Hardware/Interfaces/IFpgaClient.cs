using TftLeakage.Hardware.Models;

namespace TftLeakage.Hardware.Interfaces;

/// <summary>
/// FPGA SPI client interface for TFT panel control
/// </summary>
public interface IFpgaClient : IDisposable
{
    /// <summary>Gets a value indicating whether the client is connected</summary>
    bool IsConnected { get; }

    /// <summary>Connect to the FPGA via SPI</summary>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>Disconnect from the FPGA</summary>
    Task DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>Set idle mode on or off</summary>
    /// <param name="enabled">True to enable idle mode, false to disable</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    Task<bool> SetIdleModeAsync(bool enabled, CancellationToken cancellationToken = default);

    /// <summary>Get current bias level</summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current bias level</returns>
    Task<BiasLevel> GetBiasAsync(CancellationToken cancellationToken = default);

    /// <summary>Set bias level</summary>
    /// <param name="level">Bias level to set</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    Task<bool> SetBiasAsync(BiasLevel level, CancellationToken cancellationToken = default);

    /// <summary>Read a register</summary>
    /// <param name="address">Register address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Register value</returns>
    Task<uint> ReadRegisterAsync(byte address, CancellationToken cancellationToken = default);

    /// <summary>Write to a register</summary>
    /// <param name="address">Register address</param>
    /// <param name="value">Value to write</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    Task<bool> WriteRegisterAsync(byte address, uint value, CancellationToken cancellationToken = default);

    /// <summary>Get FPGA firmware version</summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Version string</returns>
    Task<string?> GetVersionAsync(CancellationToken cancellationToken = default);
}
