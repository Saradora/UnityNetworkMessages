using Unity.Netcode;

namespace UnityNetMessages.Events;

/// <summary>
/// Wrapper for an unmanaged message with a unique static identifier.
/// </summary>
/// <typeparam name="T">Any base unmanaged type or struct that implements IEquatable</typeparam>
public class GlobalNetMessage<T> : GlobalNetMessageBase<T> where T : unmanaged, IEquatable<T>
{
    /// <summary>
    /// Wrapper for an unmanaged message with a unique static identifier.
    /// </summary>
    /// <param name="name">The static identifier to connect the event.</param>
    /// <param name="assemblySpecific">Whether to accept sending/receiving messages with this identifier from other assemblies.</param>
    public GlobalNetMessage(string name, bool assemblySpecific = false) : base(name, assemblySpecific)
    {
    }

    protected override bool TryReadBuffer(FastBufferReader buffer, out T outValue)
    {
        outValue = default;
        if (!buffer.TryBeginReadValue(outValue))
            return false;

        buffer.ReadValue(out ForceNetworkSerializeByMemcpy<T> value);
        outValue = value.Value;
        return true;
    }

    protected override int GetWriteSize(T data)
    {
        return FastBufferWriter.GetWriteSize<T>();
    }

    protected override void WriteToBuffer(T data, FastBufferWriter writer)
    {
        writer.WriteValueSafe(new ForceNetworkSerializeByMemcpy<T>(data));
    }
}

/// <summary>
/// Wrapper for a struct network message with a unique static identifier.
/// </summary>
/// <typeparam name="T">Any unmanaged struct that implements INetworkSerializeByMemcpy</typeparam>
public class GlobalNetStructMessage<T> : GlobalNetMessageBase<T> where T : unmanaged, INetworkSerializeByMemcpy
{
    /// <summary>
    /// Wrapper for a struct network message with a unique static identifier.
    /// </summary>
    /// <param name="name">The static identifier to connect the event.</param>
    /// <param name="assemblySpecific">Whether to accept sending/receiving messages with this identifier from other assemblies.</param>
    public GlobalNetStructMessage(string name, bool assemblySpecific = false) : base(name, assemblySpecific)
    {
    }

    protected override bool TryReadBuffer(FastBufferReader buffer, out T outValue)
    {
        outValue = default;
        if (!buffer.TryBeginReadValue(outValue))
            return false;
        
        buffer.ReadValue(out outValue);
        return true;
    }

    protected override int GetWriteSize(T data)
    {
        return FastBufferWriter.GetWriteSize(data);
    }

    protected override void WriteToBuffer(T data, FastBufferWriter writer)
    {
        writer.WriteValueSafe(data);
    }
}

/// <summary>
/// Wrapper for a string network message with a unique static identifier.
/// </summary>
public class GlobalNetMessage : GlobalNetMessageBase<string>
{
    /// <summary>
    /// Wrapper for a string network message with a unique static identifier.
    /// </summary>
    /// <param name="name">The static identifier to connect the event.</param>
    /// <param name="assemblySpecific">Whether to accept sending/receiving messages with this identifier from other assemblies.</param>
    public GlobalNetMessage(string name, bool assemblySpecific = false) : base(name, assemblySpecific)
    {
    }

    protected override bool TryReadBuffer(FastBufferReader buffer, out string outValue)
    {
        buffer.ReadValueSafe(out outValue);
        return true;
    }

    protected override int GetWriteSize(string data)
    {
        return FastBufferWriter.GetWriteSize(data);
    }

    protected override void WriteToBuffer(string data, FastBufferWriter writer)
    {
        writer.WriteValueSafe(data);
    }
}