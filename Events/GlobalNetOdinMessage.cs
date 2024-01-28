using System.Diagnostics;
using OdinSerializer;
using OdinSerializer.Utilities;
using Unity.Netcode;
using UnityNetMessages.Logging;

namespace UnityNetMessages.Events;

public class GlobalNetOdinMessage<T> : GlobalNetMessageBase<T>
{
    public GlobalNetOdinMessage(string name, bool assemblySpecific = false) : base(name, assemblySpecific)
    {
        var frame = new StackFrame(2, true);
        Log.Error(frame.GetMethod().Name);
        Log.Error($"Create odin message");
    }

    private byte[] _bytes;

    protected override bool TryReadBuffer(FastBufferReader buffer, out T outValue)
    {
        Log.Error($"Try read buffer");
        int bufferLength = 0;
        if (!buffer.TryBeginReadValue(bufferLength))
        {
            Log.Error($"Couldn't read buffer length");
            outValue = default;
            return false;
        }
        
        buffer.ReadValue(out bufferLength);

        if (!buffer.TryBeginRead(bufferLength))
        {
            Log.Error($"Couldn't read buffer");
            outValue = default;
            return false;
        }

        byte[] bytes = new byte[bufferLength];
        buffer.ReadBytes(ref bytes, bufferLength);
        outValue = SerializationUtility.DeserializeValue<T>(bytes, DataFormat.Binary);
        return true;
    }

    protected override int GetWriteSize(T data)
    {
        _bytes = SerializationUtility.SerializeValue(data, DataFormat.Binary);
        return sizeof(byte) * _bytes.Length + sizeof(int);
    }

    protected override void WriteToBuffer(T data, FastBufferWriter writer)
    {
        writer.WriteValue(_bytes.Length);
        writer.WriteBytes(_bytes);
    }
}