using System.Diagnostics.CodeAnalysis;

namespace SiliconTwin.Abstractions;

/// <summary>Helpers for capability discovery on <see cref="IMcu"/>.</summary>
public static class McuExtensions
{
    /// <summary>
    /// Gets an optional capability of the MCU, for example <see cref="IGpio"/> or <see cref="IPwmSource"/>.
    /// Equivalent to <c>capability = mcu as T</c>; reflection free and AOT safe.
    /// </summary>
    /// <typeparam name="T">The capability interface.</typeparam>
    /// <param name="mcu">The MCU to query.</param>
    /// <param name="capability">The capability, or <see langword="null"/> when not supported.</param>
    /// <returns><see langword="true"/> when the MCU implements <typeparamref name="T"/>.</returns>
    public static bool TryGet<T>(this IMcu mcu, [NotNullWhen(true)] out T? capability) where T : class
    {
        ArgumentNullException.ThrowIfNull(mcu);
        capability = mcu as T;
        return capability is not null;
    }
}
