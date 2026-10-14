namespace SiliconTwin.Abstractions;

/// <summary>
/// Pure helpers to turn simulated wall time into whole cycle budgets for <see cref="IMcu.Run"/>
/// without losing fractional cycles or overshoot.
/// </summary>
/// <example>
/// <code>
/// double carry = 0;
/// // every frame:
/// long budget = CycleBudget.Next(ref carry, mcu.ClockHz, 0.001);
/// long used = mcu.Run(budget);
/// CycleBudget.Settle(ref carry, budget, used);
/// </code>
/// </example>
public static class CycleBudget
{
    /// <summary>
    /// Computes the next whole-cycle budget for <paramref name="simSeconds"/> of simulated time.
    /// </summary>
    /// <param name="carry">
    /// Fractional cycles and overshoot carried between calls, in cycles. Start it at zero and keep it
    /// across calls for the same MCU.
    /// </param>
    /// <param name="clockHz">Current clock, normally <see cref="IMcu.ClockHz"/>.</param>
    /// <param name="simSeconds">Simulated seconds to advance.</param>
    /// <returns>A budget in cycles, never negative. It can be zero when the carry is in debt.</returns>
    public static long Next(ref double carry, uint clockHz, double simSeconds)
    {
        double exact = (clockHz * simSeconds) + carry;
        long whole = exact >= 1.0 ? (long)Math.Floor(exact) : 0;
        carry = exact - whole;
        return whole;
    }

    /// <summary>
    /// Records the overshoot of a run so the next <see cref="Next"/> call pays it back.
    /// </summary>
    /// <param name="carry">The carry passed to <see cref="Next"/>.</param>
    /// <param name="budget">The budget returned by <see cref="Next"/>.</param>
    /// <param name="consumed">The value returned by <see cref="IMcu.Run"/>.</param>
    public static void Settle(ref double carry, long budget, long consumed) => carry -= consumed - budget;

    /// <summary>Converts cycles to seconds at the given clock.</summary>
    /// <param name="cycles">Cycle count or cycle delta.</param>
    /// <param name="clockHz">Clock in hertz; must be greater than zero.</param>
    public static double ToSeconds(long cycles, uint clockHz) => (double)cycles / clockHz;
}
