namespace SiliconTwin.Abstractions;

/// <summary>Selects how deep an <see cref="IMcu.Reset"/> goes.</summary>
public enum ResetKind
{
    /// <summary>
    /// Resets the CPU and the peripherals, like a reset pin or watchdog reset. Loaded firmware is kept
    /// and <see cref="IMcu.Cycles"/> keeps counting.
    /// </summary>
    Soft = 0,

    /// <summary>
    /// Behaves like removing and re-applying power. Loaded firmware is kept (it models non-volatile
    /// flash) and <see cref="IMcu.Cycles"/> restarts from zero.
    /// </summary>
    Power = 1,
}
