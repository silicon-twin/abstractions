namespace SiliconTwin.Abstractions;

/// <summary>
/// A device emulated on an I2C bus that the firmware masters. Callbacks run synchronously inside <c>Run</c>.
/// </summary>
public interface II2cTarget
{
    /// <summary>Called at the start of a transaction.</summary>
    /// <param name="addr7">7-bit address, not shifted.</param>
    /// <param name="write">True for a write transaction.</param>
    /// <returns>True to ACK the address.</returns>
    bool Probe(byte addr7, bool write);

    /// <summary>The master wrote a byte.</summary>
    /// <param name="data">The byte.</param>
    void Write(byte data);

    /// <summary>The master reads a byte.</summary>
    /// <returns>The byte to put on the bus.</returns>
    byte Read();

    /// <summary>A STOP (or repeated START) ended the transaction.</summary>
    void Stop();
}

/// <summary>A byte-level I2C controller.</summary>
public interface II2cPort
{
    /// <summary>Instance number of the peripheral.</summary>
    int Index { get; }

    /// <summary>Registers a device that answers to <paramref name="addr7"/> when the firmware is the master.</summary>
    /// <param name="addr7">7-bit address, not shifted.</param>
    /// <param name="target">The device. Replaces any previous one at the same address.</param>
    void AttachTarget(byte addr7, II2cTarget target);

    /// <summary>Removes the device at <paramref name="addr7"/>, if any.</summary>
    /// <param name="addr7">7-bit address, not shifted.</param>
    void DetachTarget(byte addr7);

    /// <summary>
    /// 7-bit address the firmware answers to when it is the slave, or 0 when the port is master only.
    /// </summary>
    byte SlaveAddress { get; }

    /// <summary>Raised when <see cref="SlaveAddress"/> changes. Zero cost with no subscriber.</summary>
    event Action<byte>? SlaveAddressChanged;

    /// <summary>
    /// Starts a transaction where an external master addresses the firmware, which acts as the slave.
    /// </summary>
    /// <param name="addr7">7-bit address, not shifted.</param>
    /// <param name="write">True when the external master writes.</param>
    /// <returns>True when the firmware ACKed the address.</returns>
    bool BeginMasterTransfer(byte addr7, bool write);

    /// <summary>The external master writes one byte to the firmware.</summary>
    /// <param name="data">The byte.</param>
    /// <returns><see langword="false"/> when the firmware cannot accept it (FIFO full or NACK).</returns>
    bool MasterWrite(byte data);

    /// <summary>The external master reads one byte from the firmware.</summary>
    /// <param name="data">The byte when the method returns <see langword="true"/>.</param>
    /// <returns><see langword="false"/> when the firmware has no byte ready.</returns>
    bool TryMasterRead(out byte data);

    /// <summary>The external master ends the transaction.</summary>
    void MasterStop();
}
