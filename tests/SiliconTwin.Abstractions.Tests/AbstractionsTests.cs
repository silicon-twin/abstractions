namespace SiliconTwin.Abstractions.Tests;

public class AbstractionsTests
{
    [Test]
    public void TryGet_returns_implemented_capability()
    {
        IMcu mcu = new FakeMcu();
        Assert.That(mcu.TryGet<IGpio>(out var gpio), Is.True);
        Assert.That(gpio!.Banks, Has.Count.EqualTo(1));
    }

    [Test]
    public void TryGet_returns_false_and_null_when_missing()
    {
        IMcu mcu = new FakeMcu();
        Assert.That(mcu.TryGet<IAdcInput>(out var adc), Is.False);
        Assert.That(adc, Is.Null);
    }

    [Test]
    public void TryGet_rejects_null_mcu()
    {
        IMcu mcu = null!;
        Assert.Throws<ArgumentNullException>(() => mcu.TryGet<IGpio>(out _));
    }

    [Test]
    public void RunUntil_default_runs_the_remaining_cycles()
    {
        var mcu = new FakeMcu();
        mcu.Run(10);
        long used = ((IMcu)mcu).RunUntil(50);
        Assert.That(used, Is.EqualTo(40));
        Assert.That(mcu.Cycles, Is.EqualTo(50));
    }

    [Test]
    public void RunUntil_default_does_nothing_when_target_is_in_the_past()
    {
        var mcu = new FakeMcu();
        mcu.Run(10);
        Assert.That(((IMcu)mcu).RunUntil(5), Is.Zero);
        Assert.That(mcu.Cycles, Is.EqualTo(10));
    }

    [Test]
    public void Run_may_overshoot_and_reports_real_cycles()
    {
        var mcu = new FakeMcu();
        Assert.That(mcu.Run(5), Is.EqualTo(6));
    }

    [Test]
    public void PinChanged_carries_the_cycle_of_the_exact_change()
    {
        var mcu = new FakeMcu();
        var seen = new List<PinChange>();
        mcu.Bank.PinChanged += (in PinChange e) => seen.Add(e);

        mcu.Run(250);

        Assert.That(seen, Has.Count.EqualTo(2));
        Assert.That(seen[0].Cycle, Is.EqualTo(100));
        Assert.That(seen[1].Cycle, Is.EqualTo(200));
        Assert.That(seen[0].IsDriven(0), Is.True);
        Assert.That(seen[0].IsHigh(0), Is.True);
        Assert.That(seen[1].IsHigh(0), Is.False);
        Assert.That(seen[0].Affects(1), Is.True);
        Assert.That(seen[0].Affects(2), Is.False);
    }

    [Test]
    public void WatchMask_zero_suppresses_events()
    {
        var mcu = new FakeMcu();
        int count = 0;
        mcu.Bank.PinChanged += (in PinChange _) => count++;
        mcu.Bank.WatchMask = 0;
        mcu.Run(250);
        Assert.That(count, Is.Zero);
    }

    [Test]
    public void SetInputs_and_ReleaseInputs_touch_only_masked_bits()
    {
        var bank = new FakeMcu().Bank;
        bank.SetInputs(0b0110, 0b0100);
        bank.SetInputs(0b0001, 0b0001);
        Assert.That(bank.ExternalLevels, Is.EqualTo(0b0101UL));
        bank.ReleaseInputs(0b0100);
        Assert.That(bank.ExternalLevels, Is.EqualTo(0b0001UL));
    }

    [Test]
    public void Power_reset_restarts_the_cycle_counter()
    {
        var mcu = new FakeMcu();
        mcu.Run(20);
        mcu.Reset(ResetKind.Soft);
        Assert.That(mcu.Cycles, Is.EqualTo(20));
        mcu.Reset(ResetKind.Power);
        Assert.That(mcu.Cycles, Is.Zero);
    }

    [Test]
    public void ClockChanged_is_raised_on_dynamic_clock()
    {
        var mcu = new FakeMcu();
        int raised = 0;
        mcu.ClockChanged += () => raised++;
        mcu.SetClock(2_000_000);
        Assert.That(raised, Is.EqualTo(1));
        Assert.That(((IMcu)mcu).ClockHz, Is.EqualTo(2_000_000u));
    }

    [Test]
    public void Load_accepts_supported_and_rejects_unsupported_formats()
    {
        var mcu = new FakeMcu();
        mcu.Load(FirmwareImage.FromBin(new byte[] { 1, 2, 3 }, 0x100));
        Assert.That(mcu.Loaded!.Value.Offset, Is.EqualTo(0x100u));
        Assert.Throws<NotSupportedException>(() => mcu.Load(FirmwareImage.FromElf(new byte[] { 0x7F })));
    }

    [Test]
    public void FromIntelHex_encodes_ascii_text()
    {
        var image = FirmwareImage.FromIntelHex(":00000001FF\n");
        Assert.That(image.Format, Is.EqualTo(FirmwareFormat.IntelHex));
        Assert.That(System.Text.Encoding.ASCII.GetString(image.Data.Span), Is.EqualTo(":00000001FF\n"));
    }

    [Test]
    public void CycleBudget_keeps_fractional_cycles_across_calls()
    {
        double carry = 0;
        long total = 0;
        for (int i = 0; i < 3; i++) total += CycleBudget.Next(ref carry, 1000, 0.0005); // 0.5 cycle each
        Assert.That(total, Is.EqualTo(1));
        Assert.That(carry, Is.EqualTo(0.5).Within(1e-9));
    }

    [Test]
    public void CycleBudget_settle_pays_back_overshoot()
    {
        double carry = 0;
        long budget = CycleBudget.Next(ref carry, 1_000_000, 0.000_010); // 10 cycles
        Assert.That(budget, Is.EqualTo(10));
        CycleBudget.Settle(ref carry, budget, 12);                        // overshoot by 2
        Assert.That(CycleBudget.Next(ref carry, 1_000_000, 0.000_010), Is.EqualTo(8));
    }

    [Test]
    public void CycleBudget_never_returns_negative()
    {
        double carry = -100;
        Assert.That(CycleBudget.Next(ref carry, 1000, 0.001), Is.Zero);
        Assert.That(carry, Is.EqualTo(-99).Within(1e-9));
    }

    [Test]
    public void CycleBudget_ToSeconds_uses_the_clock()
        => Assert.That(CycleBudget.ToSeconds(16_000_000, 16_000_000), Is.EqualTo(1.0));
}
