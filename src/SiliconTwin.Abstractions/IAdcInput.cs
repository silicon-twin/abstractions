namespace SiliconTwin.Abstractions;

/// <summary>
/// Optional MCU capability: analog inputs. The ADC pulls the value when a conversion happens, always
/// in volts; each core converts to its own resolution.
/// </summary>
public interface IAdcInput
{
    /// <summary>ADC reference voltage in volts.</summary>
    double ReferenceVolts { get; }

    /// <summary>
    /// Supplies the voltage of an ADC channel, in volts, when a conversion samples it. The callback
    /// runs synchronously inside <c>Run</c> and must be fast. When <see langword="null"/>, channels read as 0 V.
    /// </summary>
    /// <remarks>The <see cref="int"/> argument is the channel number as the device defines it.</remarks>
    Func<int, double>? ReadChannelVolts { get; set; }
}
