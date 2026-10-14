namespace SiliconTwin.Abstractions;

/// <summary>
/// The execution core of an emulated microcontroller.
/// </summary>
/// <remarks>
/// <para>
/// <b>Time.</b> Everything is measured in CPU clock cycles, never instructions or wall time. Convert
/// with <c>seconds = cycles / ClockHz</c> (see <see cref="CycleBudget"/>), reading <see cref="ClockHz"/>
/// at the moment of the conversion because it can change.
/// </para>
/// <para>
/// <b>Threading.</b> An instance has a single owner thread. Nothing here is thread safe, and every
/// callback and event raised by the MCU or by its capabilities runs synchronously on the thread that
/// called <see cref="Run"/> (or <see cref="Reset"/>, <see cref="Load"/>, or a capability setter).
/// </para>
/// <para>
/// <b>Capabilities.</b> An MCU implements <see cref="IMcu"/> plus any of <see cref="IGpio"/>,
/// <see cref="IPwmSource"/>, <see cref="IAdcInput"/> and <see cref="IPeripheralMap"/> that it supports,
/// all on the same object. Discover them with <c>is</c>/<c>as</c> or <see cref="McuExtensions.TryGet{T}"/>.
/// </para>
/// </remarks>
public interface IMcu : IDisposable
{
    /// <summary>Short identifier of the emulated device, for example <c>"atmega328p"</c> or <c>"rp2040"</c>.</summary>
    string Name { get; }

    /// <summary>
    /// CPU cycles elapsed since the last <see cref="ResetKind.Power"/> reset (or construction).
    /// Monotonic between power resets. Timestamps such as <see cref="PinChange.Cycle"/> use this scale.
    /// </summary>
    long Cycles { get; }

    /// <summary>
    /// Current CPU clock in hertz. May change at run time (PLL, prescaler), so read it each time you
    /// convert cycles to time. See <see cref="ClockChanged"/>.
    /// </summary>
    uint ClockHz { get; }

    /// <summary>
    /// Raised, synchronously inside <see cref="Run"/>, when <see cref="ClockHz"/> changes. MCUs with a
    /// fixed clock never raise it. Zero cost when nobody subscribes.
    /// </summary>
    event Action? ClockChanged;

    /// <summary>
    /// Executes until at least <paramref name="cycleBudget"/> cycles have elapsed.
    /// </summary>
    /// <param name="cycleBudget">Cycles to run, in CPU clock cycles. Values of zero or less do nothing.</param>
    /// <returns>
    /// The cycles actually consumed, which can exceed <paramref name="cycleBudget"/> by the length of
    /// the last instruction or scheduler slice. Carry the overshoot into the next budget
    /// (see <see cref="CycleBudget.Settle"/>).
    /// </returns>
    long Run(long cycleBudget);

    /// <summary>
    /// Executes until <see cref="Cycles"/> reaches <paramref name="absoluteCycle"/>.
    /// </summary>
    /// <param name="absoluteCycle">Target value of <see cref="Cycles"/>.</param>
    /// <returns>The cycles consumed; zero if the target was already reached.</returns>
    /// <remarks>The default implementation calls <see cref="Run"/> with the remaining cycles.</remarks>
    long RunUntil(long absoluteCycle)
    {
        long remaining = absoluteCycle - Cycles;
        return remaining <= 0 ? 0 : Run(remaining);
    }

    /// <summary>Resets the device.</summary>
    /// <param name="kind">How deep the reset goes.</param>
    void Reset(ResetKind kind = ResetKind.Soft);

    /// <summary>Loads firmware into program memory.</summary>
    /// <param name="image">The firmware payload.</param>
    /// <exception cref="NotSupportedException">The emulator does not support <see cref="FirmwareImage.Format"/>.</exception>
    /// <remarks>It does not reset the CPU; call <see cref="Reset"/> afterwards to start from the reset vector.</remarks>
    void Load(in FirmwareImage image);
}
