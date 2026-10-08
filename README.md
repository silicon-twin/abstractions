# SiliconTwin.Abstractions

[![CI](https://github.com/silicon-twin/abstractions/actions/workflows/ci.yml/badge.svg)](https://github.com/silicon-twin/abstractions/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/SiliconTwin.Abstractions.svg)](https://www.nuget.org/packages/SiliconTwin.Abstractions)
![License](https://img.shields.io/badge/license-MIT-green.svg)
![.NET Version](https://img.shields.io/badge/.NET-10.0-purple)

Shared contracts for microcontroller emulators written in C#. One small, dependency-free package that
lets a host (a circuit simulator, a test kit, a UI) drive an AVR, an RP2040 or any other emulated MCU
through the same interfaces.

- No dependencies, no reflection, trim and AOT safe.
- MIT licensed, so emulator cores of any license can reference it.
- Contracts only. Parsers (Intel HEX, UF2, ELF), debuggers and snapshots are out of scope for 0.1.

```
dotnet add package SiliconTwin.Abstractions
```

## Design rules

- **Cycles, not instructions or seconds.** `IMcu.Run(long cycleBudget)` returns the cycles actually
  consumed (it can overshoot by one instruction). Time is `cycles / ClockHz`, and `ClockHz` can change.
- **Timestamps are cycles.** Every `PinChange` carries `IMcu.Cycles` at the exact instant of the change.
- **One owner thread.** Nothing is thread safe. Events and callbacks fire synchronously inside `Run`.
- **Zero cost when unused.** `PinChanged` does nothing without subscribers, and `WatchMask` narrows
  the pins that are tracked. Optional capabilities are only implemented by MCUs that have them.
- **Masks are `ulong`.** A pin bank has at most 64 pins; bit `n` is pin `n`. An AVR port is one bank
  of 8 pins, an RP2040 has one bank of 30.

## Contract overview

| Type | Purpose |
|---|---|
| `IMcu` | `Name`, `Cycles`, `ClockHz`, `ClockChanged`, `Run`, `RunUntil`, `Reset(ResetKind)`, `Load(FirmwareImage)` |
| `FirmwareImage`, `FirmwareFormat` | Firmware bytes plus format (`IntelHex`, `Uf2`, `Elf`, `Bin`, `Auto`) and load offset |
| `IGpio` | Optional. `Banks`: the list of `IPinBank` |
| `IPinBank` | `OutputEnableMask`, `OutputLevels`, `PullUpMask`, `PullDownMask` (chip side); `SetInputs`, `ReleaseInputs` (circuit side); `PinChanged`, `WatchMask` |
| `PinChange` | `Changed`, `OutputEnable`, `Levels`, `Cycle` for a batch of pins in one bank |
| `IPwmSource` | Optional. `TryGetPwm(bank, pin, out PwmInfo)` |
| `IAdcInput` | Optional. `ReferenceVolts` and the `ReadChannelVolts` callback, always in volts |
| `IPeripheralMap` | Optional. `Uarts`, `Spis`, `I2cs` |
| `IUartPort`, `ISpiPort`, `II2cPort` | Byte-level buses |
| `CycleBudget` | Turns simulated seconds into whole cycle budgets, carrying fractions and overshoot |

## Capability discovery

An emulator is one object that implements `IMcu` plus the capabilities it supports. Use `is`/`as`, or the
`TryGet` helper:

```csharp
using SiliconTwin.Abstractions;

if (mcu.TryGet<IGpio>(out var gpio))
    Console.WriteLine($"{gpio.Banks.Count} pin banks");

if (mcu is IPwmSource pwm && pwm.TryGetPwm(bank: 0, pin: 3, out var info) && info.Enabled)
    Console.WriteLine($"{info.FrequencyHz:F0} Hz, duty {info.Duty:P0}");
```

## Consuming an MCU

```csharp
using SiliconTwin.Abstractions;

void Blink(IMcu mcu, string hex)
{
    mcu.Load(FirmwareImage.FromIntelHex(hex));
    mcu.Reset(ResetKind.Power);

    if (!mcu.TryGet<IGpio>(out var gpio)) return;
    IPinBank portB = gpio.Banks[1];
    portB.WatchMask = 1UL << 5;                       // only pin 5 (Arduino pin 13)

    long last = 0;
    portB.PinChanged += (in PinChange e) =>
    {
        if (!e.IsDriven(5)) return;
        double ms = CycleBudget.ToSeconds(e.Cycle - last, mcu.ClockHz) * 1000;
        Console.WriteLine($"pin 5 -> {(e.IsHigh(5) ? "HIGH" : "LOW")} after {ms:F3} ms");
        last = e.Cycle;
    };

    portB.SetInputs(1UL << 0, 1UL << 0);              // drive pin 0 high from the circuit
    portB.ReleaseInputs(1UL << 0);                    // back to high impedance

    double carry = 0;
    for (int frame = 0; frame < 1000; frame++)        // 1 s of simulated time in 1 ms steps
    {
        long budget = CycleBudget.Next(ref carry, mcu.ClockHz, 0.001);
        long used = mcu.Run(budget);
        CycleBudget.Settle(ref carry, budget, used);
    }
}
```

## Implementing it in an emulator

Add the package reference, implement `IMcu` on a thin wrapper around your machine, and add the
capability interfaces you support on the same class:

```csharp
public sealed class MyMcu : IMcu, IGpio
{
    readonly MyMachine _m = new();
    readonly MyPinBank[] _banks = [new MyPinBank()];

    public string Name => "mychip";
    public long Cycles => (long)_m.Cycles;              // convert if your core uses ulong
    public uint ClockHz => _m.ClockHz;
    public event Action? ClockChanged;                  // never raised if the clock is fixed

    public long Run(long cycleBudget)
    {
        long start = Cycles;
        while (Cycles - start < cycleBudget) _m.Step();
        return Cycles - start;                          // may exceed the budget
    }

    public void Reset(ResetKind kind = ResetKind.Soft) => _m.Reset(power: kind == ResetKind.Power);
    public void Load(in FirmwareImage image) { /* parse image.Data per image.Format, or throw NotSupportedException */ }
    public IReadOnlyList<IPinBank> Banks => _banks;
    public void Dispose() => _m.Dispose();
}
```

Rules for implementers:

- `PinChanged` must cost nothing without subscribers: use `add`/`remove` accessors to arm the hook,
  and skip pins outside `WatchMask`.
- `OutputEnableMask` and `OutputLevels` are the effective pad state after the function mux, so a timer
  output (for example OC0A) counts. `OutputLevels` is meaningful only where `OutputEnableMask` is set.
- Raise events and call callbacks synchronously, on the thread that called `Run`.
- Throw `NotSupportedException` from `Load` for formats you cannot read.

## Publishing (maintainers)

Pushing a tag `vX.Y.Z` that matches `<Version>` in the csproj runs `.github/workflows/publish.yml`,
which pushes to nuget.org with Trusted Publishing (OIDC), without a long-lived API key. One-time setup:

1. GitHub repo variable `NUGET_USER` set to the nuget.org account that owns the package.
2. A trusted-publishing policy on nuget.org for repository `silicon-twin/abstractions`, workflow file `publish.yml`.

## License

MIT. See [LICENSE](LICENSE).
