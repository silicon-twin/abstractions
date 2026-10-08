# Changelog

All notable changes to **SiliconTwin.Abstractions** are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
Until 1.0.0 the API may still change between minor versions.

## [0.1.0]

First release. Contracts only, no dependencies, trim and AOT safe, `net10.0`.

### Added

- `IMcu`: `Name`, `Cycles`, `ClockHz`, `ClockChanged`, `Run(long cycleBudget)`, `RunUntil`, `Reset(ResetKind)`, `Load(FirmwareImage)`.
- `ResetKind` (`Soft`, `Power`), `FirmwareFormat` (`Auto`, `IntelHex`, `Uf2`, `Elf`, `Bin`) and `FirmwareImage`.
- GPIO: `IGpio`, `IPinBank` (`OutputEnableMask`, `OutputLevels`, `PullUpMask`, `PullDownMask`, `SetInputs`, `ReleaseInputs`, `PinChanged`, `WatchMask`), `PinChange`, `PinChangeHandler`.
- `IPwmSource` and `PwmInfo`, `IAdcInput` (`ReadChannelVolts`).
- Byte-level buses: `IUartPort`, `ISpiPort`, `II2cPort`, `II2cTarget` and `IPeripheralMap`.
- `McuExtensions.TryGet<T>` for capability discovery and the `CycleBudget` helper.

### Not included yet

- Debugging (`IDebugTarget`) and snapshots (`ISnapshotable`) are reserved for later versions.
