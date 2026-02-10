using System.IO.Ports;
using TftLeakage.Hardware.Interfaces;
using TftLeakage.Hardware.Models;

namespace TftLeakage.Hardware;

/// <summary>
/// FPGA SPI client implementation for TFT panel control
/// </summary>
public class FpgaSpiClient : IFpgaClient
{
    private const int DefaultMaxSpeedHz = 25000000; // 25 MHz
    private readonly string _spiDevice;
    private readonly int _maxSpeedHz;
    private FileStream? _spiDeviceStream;

    /// <summary>
    /// Initialize a new FpgaSpiClient
    /// </summary>
    /// <param name="spiDevice">SPI device path (e.g., "/dev/spidev0.0")</param>
    /// <param name="maxSpeedHz">Maximum SPI clock speed in Hz</param>
    public FpgaSpiClient(string spiDevice = "/dev/spidev0.0", int maxSpeedHz = DefaultMaxSpeedHz)
    {
        _spiDevice = spiDevice;
        _maxSpeedHz = maxSpeedHz;
    }

    /// <summary>
    /// Initialize a new FpgaSpiClient with bus and device numbers
    /// </summary>
    /// <param name="bus">SPI bus number</param>
    /// <param name="device">SPI device number on the bus</param>
    public FpgaSpiClient(int bus, int device)
        : this($"/dev/spidev{bus}.{device}", DefaultMaxSpeedHz)
    {
    }

    /// <inheritdoc/>
    public bool IsConnected => _spiDeviceStream != null;

    /// <inheritdoc/>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected)
            return;

        await Task.Run(() =>
        {
            _spiDeviceStream = File.OpenWrite(_spiDevice);
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
            return;

        await Task.Run(() =>
        {
            _spiDeviceStream?.Dispose();
            _spiDeviceStream = null;
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> SetIdleModeAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        var value = enabled ? CtrlBits.IDLE_EN : 0u;
        return await WriteRegisterAsync(FpgaRegisters.CTRL, value, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<BiasLevel> GetBiasAsync(CancellationToken cancellationToken = default)
    {
        var value = await ReadRegisterAsync(FpgaRegisters.BIAS_SEL, cancellationToken);
        return (BiasLevel)(value & 0x7);
    }

    /// <inheritdoc/>
    public async Task<bool> SetBiasAsync(BiasLevel level, CancellationToken cancellationToken = default)
    {
        return await WriteRegisterAsync(FpgaRegisters.BIAS_SEL, (uint)level, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<uint> ReadRegisterAsync(byte address, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        return await Task.Run(() =>
        {
            // SPI transaction: write address + dummy data, then read response
            var writeBuffer = new byte[5];
            writeBuffer[0] = address;
            // Address first, then 32-bit data (MSB first)
            writeBuffer[1] = 0x00;
            writeBuffer[2] = 0x00;
            writeBuffer[3] = 0x00;
            writeBuffer[4] = 0x00;

            var readBuffer = new byte[5];

            _spiDeviceStream!.Write(writeBuffer, 0, writeBuffer.Length);
            _spiDeviceStream.Flush();

            // Small delay for transaction to complete
            Thread.Sleep(1);

            _spiDeviceStream.Read(readBuffer, 0, readBuffer.Length);

            // Convert 4 bytes to uint (big-endian)
            return ((uint)readBuffer[1] << 24) |
                   ((uint)readBuffer[2] << 16) |
                   ((uint)readBuffer[3] << 8) |
                   readBuffer[4];
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> WriteRegisterAsync(byte address, uint value, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        try
        {
            await Task.Run(() =>
            {
                // SPI write: address (8-bit) + data (32-bit MSB first)
                var writeBuffer = new byte[5];
                writeBuffer[0] = address;
                writeBuffer[1] = (byte)((value >> 24) & 0xFF);
                writeBuffer[2] = (byte)((value >> 16) & 0xFF);
                writeBuffer[3] = (byte)((value >> 8) & 0xFF);
                writeBuffer[4] = (byte)(value & 0xFF);

                _spiDeviceStream!.Write(writeBuffer, 0, writeBuffer.Length);
                _spiDeviceStream.Flush();
            }, cancellationToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<string?> GetVersionAsync(CancellationToken cancellationToken = default)
    {
        var version = await ReadRegisterAsync(FpgaRegisters.VERSION, cancellationToken);
        if (version == 0)
            return null;

        // Convert to string (e.g., 0x56313030 -> "V10")
        var chars = new char[4];
        for (int i = 0; i < 4; i++)
        {
            var b = (version >> (8 * (3 - i))) & 0xFF;
            if (b >= 32 && b <= 126)
                chars[i] = (char)b;
            else
                chars[i] = '?';
        }
        return new string(chars);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _spiDeviceStream?.Dispose();
        _spiDeviceStream = null;
        GC.SuppressFinalize(this);
    }

    private void EnsureConnected()
    {
        if (!IsConnected)
            throw new InvalidOperationException("Not connected to SPI device. Call ConnectAsync first.");
    }
}
