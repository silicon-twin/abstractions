namespace SiliconTwin.Abstractions;

/// <summary>Describes the PWM signal on a pin.</summary>
/// <param name="Enabled">True when a PWM output is active on the pin.</param>
/// <param name="FrequencyHz">Frequency of the PWM signal in hertz of simulated time.</param>
/// <param name="Duty">Duty cycle from 0.0 (always low) to 1.0 (always high).</param>
public readonly record struct PwmInfo(bool Enabled, double FrequencyHz, double Duty);

/// <summary>
/// Optional MCU capability: reports the PWM signals that peripherals generate on pins. Polled; it
/// raises no events.
/// </summary>
public interface IPwmSource
{
    /// <summary>Gets the PWM state of a pin.</summary>
    /// <param name="bank">Index into <see cref="IGpio.Banks"/>.</param>
    /// <param name="pin">Pin index within the bank.</param>
    /// <param name="info">The PWM description when the method returns <see langword="true"/>.</param>
    /// <returns>
    /// <see langword="false"/> when the pin has no PWM-capable source routed to it. A
    /// <see langword="true"/> result with <see cref="PwmInfo.Enabled"/> unset means a PWM source is
    /// routed but stopped.
    /// </returns>
    bool TryGetPwm(int bank, int pin, out PwmInfo info);
}
