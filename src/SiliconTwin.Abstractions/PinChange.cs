namespace SiliconTwin.Abstractions;

/// <summary>
/// A batch of pin changes in one <see cref="IPinBank"/>, raised by <see cref="IPinBank.PinChanged"/>.
/// All masks use bit <c>n</c> for pin <c>n</c> of the bank.
/// </summary>
/// <param name="Changed">Pins whose effective level or output-enable state changed.</param>
/// <param name="OutputEnable">Pins the chip drives, after the change. A released pin (input, open drain high) is zero.</param>
/// <param name="Levels">
/// Effective pad level driven by the chip, after the change, as seen through the function mux (a timer
/// output counts). Meaningful only where <paramref name="OutputEnable"/> is set.
/// </param>
/// <param name="Cycle">The value of <see cref="IMcu.Cycles"/> at the exact instant of the change. Use it for sub-microsecond timing.</param>
public readonly record struct PinChange(ulong Changed, ulong OutputEnable, ulong Levels, long Cycle)
{
    /// <summary>True when any pin in <paramref name="mask"/> is part of this change.</summary>
    /// <param name="mask">Pins of interest.</param>
    public bool Affects(ulong mask) => (Changed & mask) != 0;

    /// <summary>True when the chip drives <paramref name="pin"/> after the change.</summary>
    /// <param name="pin">Pin index within the bank, 0 to 63.</param>
    public bool IsDriven(int pin) => ((OutputEnable >> pin) & 1UL) != 0;

    /// <summary>Level of <paramref name="pin"/> after the change. Only meaningful when <see cref="IsDriven"/> is true.</summary>
    /// <param name="pin">Pin index within the bank, 0 to 63.</param>
    public bool IsHigh(int pin) => ((Levels >> pin) & 1UL) != 0;
}

/// <summary>Handler for <see cref="IPinBank.PinChanged"/>.</summary>
/// <param name="e">Details of the change. Passed by reference to avoid copies; do not keep the reference.</param>
public delegate void PinChangeHandler(in PinChange e);
