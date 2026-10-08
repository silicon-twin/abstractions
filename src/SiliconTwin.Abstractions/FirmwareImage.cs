using System.Text;

namespace SiliconTwin.Abstractions;

/// <summary>
/// A firmware payload handed to <see cref="IMcu.Load"/>. This package only carries the bytes; it does
/// not parse any format. Each emulator decides which formats it supports.
/// </summary>
/// <param name="Format">Container format of <paramref name="Data"/>.</param>
/// <param name="Data">
/// The payload. For <see cref="FirmwareFormat.IntelHex"/> this is the ASCII text of the HEX file.
/// </param>
/// <param name="Offset">
/// Load address for <see cref="FirmwareFormat.Bin"/>, relative to the start of the program memory
/// (flash) of the target. Ignored by self-describing formats.
/// </param>
public readonly record struct FirmwareImage(FirmwareFormat Format, ReadOnlyMemory<byte> Data, uint Offset = 0)
{
    /// <summary>Creates an Intel HEX image from its text.</summary>
    /// <param name="hex">Content of the .hex file.</param>
    public static FirmwareImage FromIntelHex(string hex)
    {
        ArgumentNullException.ThrowIfNull(hex);
        return new FirmwareImage(FirmwareFormat.IntelHex, Encoding.ASCII.GetBytes(hex));
    }

    /// <summary>Creates a UF2 image.</summary>
    /// <param name="uf2">Content of the .uf2 file.</param>
    public static FirmwareImage FromUf2(ReadOnlyMemory<byte> uf2) => new(FirmwareFormat.Uf2, uf2);

    /// <summary>Creates an ELF image.</summary>
    /// <param name="elf">Content of the ELF file.</param>
    public static FirmwareImage FromElf(ReadOnlyMemory<byte> elf) => new(FirmwareFormat.Elf, elf);

    /// <summary>Creates a raw binary image.</summary>
    /// <param name="bin">The raw bytes.</param>
    /// <param name="offset">Load offset from the start of program memory.</param>
    public static FirmwareImage FromBin(ReadOnlyMemory<byte> bin, uint offset = 0) => new(FirmwareFormat.Bin, bin, offset);
}
