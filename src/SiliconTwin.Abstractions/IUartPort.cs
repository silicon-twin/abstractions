namespace SiliconTwin.Abstractions;

/// <summary>A byte-level UART. Framing, parity and timing are the emulator's business.</summary>
public interface IUartPort
{
    /// <summary>Instance number of the peripheral (UART0 is 0).</summary>
    int Index { get; }

    /// <summary>Configured baud rate, or 0 when the peripheral is not configured.</summary>
    uint BaudRate { get; }

    /// <summary>
    /// Raised, synchronously inside <c>Run</c>, for every byte the firmware transmits. Zero cost with no subscriber.
    /// </summary>
    event Action<byte>? TxByte;

    /// <summary>Delivers a byte to the receiver of the firmware.</summary>
    /// <param name="value">The received byte.</param>
    /// <returns><see langword="false"/> when the receive FIFO is full and the byte was dropped.</returns>
    bool TryInjectRx(byte value);
}
