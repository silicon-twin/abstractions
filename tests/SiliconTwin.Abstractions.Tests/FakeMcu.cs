namespace SiliconTwin.Abstractions.Tests;

/// <summary>A tiny MCU that burns 2 cycles per step and toggles pin 0 of one bank every 100 cycles.</summary>
internal sealed class FakeMcu : IMcu, IGpio
{
    private readonly FakePinBank _bank;
    private long _nextToggle = 100;

    public FakeMcu() => _bank = new FakePinBank(this);

    public string Name => "fake";
    public long Cycles { get; private set; }
    public uint ClockHz { get; private set; } = 1_000_000;
    public event Action? ClockChanged;
    public IReadOnlyList<IPinBank> Banks => new IPinBank[] { _bank };
    public FakePinBank Bank => _bank;
    public FirmwareImage? Loaded { get; private set; }

    public long Run(long cycleBudget)
    {
        long start = Cycles;
        while (Cycles - start < cycleBudget)
        {
            Cycles += 2;
            if (Cycles >= _nextToggle)
            {
                _nextToggle += 100;
                _bank.ToggleOutput0();
            }
        }
        return Cycles - start;
    }

    public void SetClock(uint hz)
    {
        ClockHz = hz;
        ClockChanged?.Invoke();
    }

    public void Reset(ResetKind kind = ResetKind.Soft)
    {
        if (kind == ResetKind.Power) { Cycles = 0; _nextToggle = 100; }
    }

    public void Load(in FirmwareImage image)
    {
        if (image.Format == FirmwareFormat.Elf) throw new NotSupportedException("ELF");
        Loaded = image;
    }

    public void Dispose() { }
}

internal sealed class FakePinBank : IPinBank
{
    private readonly FakeMcu _mcu;
    private PinChangeHandler? _changed;
    private ulong _oe = 1, _levels, _forced, _forcedLevels;

    public FakePinBank(FakeMcu mcu) { _mcu = mcu; WatchMask = ulong.MaxValue; }

    public string Name => "PORTA";
    public int PinCount => 8;
    public ulong OutputEnableMask => _oe;
    public ulong OutputLevels => _levels;
    public ulong PullUpMask => 0;
    public ulong PullDownMask => 0;
    public ulong WatchMask { get; set; }
    public ulong ExternalLevels => _forcedLevels & _forced;

    public event PinChangeHandler PinChanged
    {
        add => _changed += value;
        remove => _changed -= value;
    }

    public void SetInputs(ulong mask, ulong levels)
    {
        _forced |= mask;
        _forcedLevels = (_forcedLevels & ~mask) | (levels & mask);
    }

    public void ReleaseInputs(ulong mask)
    {
        _forced &= ~mask;
        _forcedLevels &= ~mask;
    }

    public void ToggleOutput0()
    {
        _levels ^= 1;
        if (_changed is null || (WatchMask & 1) == 0) return;
        _changed(new PinChange(1, _oe, _levels, _mcu.Cycles));
    }
}
