namespace SiliconTwin.Abstractions;

/// <summary>Container format of the bytes carried by a <see cref="FirmwareImage"/>.</summary>
public enum FirmwareFormat
{
    /// <summary>The emulator detects the format from the content. Emulators may reject it.</summary>
    Auto = 0,

    /// <summary>Intel HEX text, ASCII encoded (used by AVR toolchains).</summary>
    IntelHex = 1,

    /// <summary>UF2 blocks (used by RP2040 and other UF2 bootloaders).</summary>
    Uf2 = 2,

    /// <summary>ELF executable.</summary>
    Elf = 3,

    /// <summary>Raw binary placed at <see cref="FirmwareImage.Offset"/>.</summary>
    Bin = 4,
}
