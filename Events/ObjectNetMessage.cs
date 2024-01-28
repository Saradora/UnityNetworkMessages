using Unity.Netcode;

namespace UnityNetMessages.Events;

/// <summary>
/// Wrapper for an unmanaged message with a unique identifier attached to a NetworkObject.
/// </summary>
/// <typeparam name="T">Any base unmanaged type or struct that implements IEquatable</typeparam>
public class ObjectNetMessage<T> : ObjectNetMessageBase<T> where T : unmanaged, IEquatable<T>
{
    /// <summary>
    /// Wrapper for an unmanaged message with a unique identifier attached to a NetworkObject.
    /// </summary>
    /// <param name="name">The static identifier to connect the event.</param>
    /// <param name="assemblySpecific">Whether to accept sending/receiving messages with this identifier from other assemblies.</param>
    public ObjectNetMessage(string name, NetworkObject targetObject, bool assemblySpecific = false) : base(name, targetObject, assemblySpecific)
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

public class ObjectNetStructMessage<T> : ObjectNetMessageBase<T> where T : unmanaged, INetworkSerializeByMemcpy
{
    public ObjectNetStructMessage(string name, NetworkObject targetObject, bool assemblySpecific = false) : base(name, targetObject, assemblySpecific)
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

public class ObjectNetMessage : ObjectNetMessageBase<string>
{
    public ObjectNetMessage(string name, NetworkObject targetObject, bool assemblySpecific = false) : base(name, targetObject, assemblySpecific)
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