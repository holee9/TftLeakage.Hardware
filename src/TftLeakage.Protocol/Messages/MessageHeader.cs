namespace TftLeakage.Protocol.Messages;

/// <summary>
/// TFT protocol message header
/// </summary>
public class MessageHeader
{
    private const uint MagicValue = 0x54465400; // "TFT\0"
    private const int HeaderSize = 8;

    /// <summary>Magic number for protocol identification (0x54465400)</summary>
    public uint Magic { get; set; } = MagicValue;

    /// <summary>Message type</summary>
    public byte Type { get; set; }

    /// <summary>Message payload length</summary>
    public ushort Length { get; set; }

    /// <summary>Message sequence number</summary>
    public byte Sequence { get; set; }

    /// <summary>Total header size in bytes</summary>
    public static int Size => HeaderSize;

    /// <summary>
    /// Serialize header to bytes
    /// </summary>
    public byte[] ToBytes()
    {
        var buffer = new byte[HeaderSize];
        buffer[0] = (byte)((Magic >> 24) & 0xFF);
        buffer[1] = (byte)((Magic >> 16) & 0xFF);
        buffer[2] = (byte)((Magic >> 8) & 0xFF);
        buffer[3] = (byte)(Magic & 0xFF);
        buffer[4] = Type;
        buffer[5] = (byte)((Length >> 8) & 0xFF);
        buffer[6] = (byte)(Length & 0xFF);
        buffer[7] = Sequence;
        return buffer;
    }

    /// <summary>
    /// Parse header from bytes
    /// </summary>
    public static MessageHeader FromBytes(byte[] buffer)
    {
        if (buffer.Length < HeaderSize)
            throw new ArgumentException("Buffer too small for header");

        var header = new MessageHeader
        {
            Magic = (uint)((buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | buffer[3]),
            Type = buffer[4],
            Length = (ushort)((buffer[5] << 8) | buffer[6]),
            Sequence = buffer[7]
        };

        if (header.Magic != MagicValue)
            throw new InvalidDataException($"Invalid magic number: 0x{header.Magic:X8}");

        return header;
    }

    /// <summary>
    /// Check if magic number is valid
    /// </summary>
    public bool IsValidMagic() => Magic == MagicValue;
}
