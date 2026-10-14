namespace SiliconTwin.Abstractions;

/// <summary>
/// A group of up to 64 pins that change together, such as an AVR port or an RP2040 GPIO bank.
/// Bit <c>n</c> of every mask is pin <c>n</c> of the bank.
/// </summary>
/// <remarks>
/// <para>
/// Two sides exist. The <i>chip side</i> (<see cref="OutputEnableMask"/>, <see cref="OutputLevels"/>,
/// pulls) is what the firmware makes the pad do, resolved through the function mux, so a timer or
/// PIO output counts. The <i>external side</i> (<see cref="SetInputs"/>, <see cref="ReleaseInputs"/>)
/// is what the surrounding circuit applies to the pad.
/// </para>
/// <para>
/// Callbacks run synchronously on the thread that runs the MCU. Not thread safe.
/// </para>
/// </remarks>
public interface IPinBank
{
    /// <summary>Name of the bank, for example <c>"PORTB"</c> or <c>"GPIO"</c>.</summary>
    string Name { get; }

    /// <summary>Number of pins in the bank, 1 to 64.</summary>
    int PinCount { get; }

    /// <summary>Pins the chip currently drives (output enable, after the function mux).</summary>
    ulong OutputEnableMask { get; }

    /// <summary>
    /// Effective level the chip drives on each pad. Meaningful only for bits set in
    /// <see cref="OutputEnableMask"/>; other bits are unspecified.
    /// </summary>
    ulong OutputLevels { get; }

    /// <summary>Pins with the internal pull-up enabled.</summary>
    ulong PullUpMask { get; }

    /// <summary>Pins with the internal pull-down enabled. Zero on devices without pull-downs.</summary>
    ulong PullDownMask { get; }

    /// <summary>
    /// Forces the external level of the pins in <paramref name="mask"/>; bits outside it are untouched.
    /// </summary>
    /// <param name="mask">Pins to drive from outside.</param>
    /// <param name="levels">Level for each pin in <paramref name="mask"/> (1 = high). Other bits are ignored.</param>
    void SetInputs(ulong mask, ulong levels);

    /// <summary>
    /// Stops driving the pins in <paramref name="mask"/> from outside (external high impedance), so
    /// the internal pull, if any, decides the level.
    /// </summary>
    /// <param name="mask">Pins to release.</param>
    void ReleaseInputs(ulong mask);

    /// <summary>
    /// Raised, synchronously during <c>Run</c>, when the effective level or output-enable of a pin
    /// in <see cref="WatchMask"/> changes. Implementations must cost nothing while there is no
    /// subscriber and nothing for pins outside <see cref="WatchMask"/>.
    /// </summary>
    event PinChangeHandler PinChanged;

    /// <summary>
    /// Pins that may raise <see cref="PinChanged"/>. Defaults to all pins of the bank. Narrow it to
    /// skip work for pins nobody observes.
    /// </summary>
    ulong WatchMask { get; set; }
}
