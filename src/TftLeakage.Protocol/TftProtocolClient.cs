using System.Net.Sockets;
using TftLeakage.Hardware.Models;
using TftLeakage.Protocol.Messages;
using TftLeakage.Protocol.Models;

namespace TftLeakage.Protocol;

/// <summary>
/// TFT Ethernet protocol client
/// </summary>
public class TftProtocolClient : IDisposable
{
    private const int DefaultPort = 8080;
    private const int ReceiveTimeout = 5000; // 5 seconds
    private const int SendTimeout = 5000;
    private readonly string _host;
    private readonly int _port;
    private TcpClient? _tcpClient;
    private byte _sequence = 0;

    /// <summary>
    /// Initialize a new TftProtocolClient
    /// </summary>
    /// <param name="host">Target host IP address or hostname</param>
    /// <param name="port">TCP port number</param>
    public TftProtocolClient(string host, int port = DefaultPort)
    {
        _host = host;
        _port = port;
    }

    /// <summary>Gets a value indicating whether the client is connected</summary>
    public bool IsConnected => _tcpClient?.Connected == true;

    /// <summary>Connect to the TFT protocol server</summary>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected)
            return;

        _tcpClient = new TcpClient
        {
            ReceiveTimeout = ReceiveTimeout,
            SendTimeout = SendTimeout
        };

        await _tcpClient.ConnectAsync(_host, _port, cancellationToken);
    }

    /// <summary>Disconnect from the server</summary>
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
            return;

        await Task.Run(() =>
        {
            _tcpClient?.Close();
            _tcpClient = null;
        }, cancellationToken);
    }

    /// <summary>Get system status</summary>
    public async Task<SystemStatus?> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        var header = new MessageHeader
        {
            Type = (byte)MessageType.StatusRequest,
            Length = 0,
            Sequence = GetNextSequence()
        };

        await SendMessageAsync(header, null, cancellationToken);

        var response = await ReceiveMessageAsync(cancellationToken);
        if (response.header.Type == (byte)MessageType.StatusResponse)
        {
            return StatusResponse.FromBytes(response.payload).ToSystemStatus();
        }

        return null;
    }

    /// <summary>Set idle mode</summary>
    public async Task<bool> SetIdleModeAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        var payload = new byte[1];
        payload[0] = (byte)(enabled ? 1 : 0);

        var header = new MessageHeader
        {
            Type = (byte)MessageType.SetIdleMode,
            Length = 1,
            Sequence = GetNextSequence()
        };

        await SendMessageAsync(header, payload, cancellationToken);

        var response = await ReceiveMessageAsync(cancellationToken);
        return response.header.Type != (byte)MessageType.ErrorResponse;
    }

    /// <summary>Set bias level</summary>
    public async Task<bool> SetBiasAsync(BiasLevel level, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        var payload = new byte[1];
        payload[0] = (byte)level;

        var header = new MessageHeader
        {
            Type = (byte)MessageType.SetBiasLevel,
            Length = 1,
            Sequence = GetNextSequence()
        };

        await SendMessageAsync(header, payload, cancellationToken);

        var response = await ReceiveMessageAsync(cancellationToken);
        return response.header.Type != (byte)MessageType.ErrorResponse;
    }

    /// <summary>Get current bias level</summary>
    public async Task<BiasLevel> GetBiasAsync(CancellationToken cancellationToken = default)
    {
        var status = await GetStatusAsync(cancellationToken);
        return status?.CurrentBias ?? Hardware.Models.BiasLevel.Off;
    }

    private async Task SendMessageAsync(MessageHeader header, byte[]? payload, CancellationToken cancellationToken)
    {
        var stream = _tcpClient!.GetStream();

        var headerBytes = header.ToBytes();
        await stream.WriteAsync(headerBytes, cancellationToken);

        if (payload != null && payload.Length > 0)
        {
            await stream.WriteAsync(payload, cancellationToken);
        }

        await stream.FlushAsync(cancellationToken);
    }

    private async Task<(MessageHeader header, byte[] payload)> ReceiveMessageAsync(CancellationToken cancellationToken)
    {
        var stream = _tcpClient!.GetStream();

        // Read header
        var headerBuffer = new byte[MessageHeader.Size];
        var bytesRead = await stream.ReadAsync(headerBuffer, cancellationToken);
        if (bytesRead < MessageHeader.Size)
            throw new IOException("Incomplete header received");

        var header = MessageHeader.FromBytes(headerBuffer);

        // Read payload if present
        byte[] payload = Array.Empty<byte>();
        if (header.Length > 0)
        {
            payload = new byte[header.Length];
            bytesRead = await stream.ReadAsync(payload, cancellationToken);
            if (bytesRead < header.Length)
                throw new IOException("Incomplete payload received");
        }

        return (header, payload);
    }

    private byte GetNextSequence() => _sequence++;

    private void EnsureConnected()
    {
        if (!IsConnected)
            throw new InvalidOperationException("Not connected. Call ConnectAsync first.");
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _tcpClient?.Dispose();
        _tcpClient = null;
        GC.SuppressFinalize(this);
    }
}
