using Unity.Netcode;

namespace UnityNetMessages.Events;

public class ObjectNetMessage<T> : ObjectNetMessageBase<T> where T : unmanaged, IEquatable<T>
{
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