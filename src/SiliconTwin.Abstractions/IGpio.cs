namespace SiliconTwin.Abstractions;

/// <summary>
/// Optional MCU capability: digital pins, grouped in banks (an AVR port, an RP2040 GPIO bank, a
/// Cortex-M GPIO port).
/// </summary>
public interface IGpio
{
    /// <summary>
    /// The pin banks of the device. The list is fixed for the lifetime of the MCU; the bank index used
    /// by <see cref="IPwmSource"/> is the position in this list.
    /// </summary>
    IReadOnlyList<IPinBank> Banks { get; }
}
