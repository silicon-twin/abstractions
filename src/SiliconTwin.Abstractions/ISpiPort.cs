namespace SiliconTwin.Abstractions;

/// <summary>A byte-level SPI controller where the firmware is the master.</summary>
public interface ISpiPort
{
    /// <summary>Instance number of the peripheral.</summary>
    int Index { get; }

    /// <summary>Bits per frame: 8 on AVR, 4 to 16 on RP2040.</summary>
    int FrameBits { get; }

    /// <summary>
    /// Called synchronously inside <c>Run</c> for every frame the firmware shifts out. The argument is
    /// the MOSI frame and the return value is the MISO frame, both right aligned in <see cref="FrameBits"/> bits.
    /// When <see langword="null"/> the bus is idle and the firmware reads all ones.
    /// </summary>
    Func<uint, uint>? Transfer { get; set; }
}
