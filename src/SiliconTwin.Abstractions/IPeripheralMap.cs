namespace SiliconTwin.Abstractions;

/// <summary>
/// Optional MCU capability: the byte-level buses of the device. Lists are fixed for the lifetime of
/// the MCU and may be empty.
/// </summary>
public interface IPeripheralMap
{
    /// <summary>UART peripherals.</summary>
    IReadOnlyList<IUartPort> Uarts { get; }

    /// <summary>SPI peripherals.</summary>
    IReadOnlyList<ISpiPort> Spis { get; }

    /// <summary>I2C peripherals.</summary>
    IReadOnlyList<II2cPort> I2cs { get; }
}
