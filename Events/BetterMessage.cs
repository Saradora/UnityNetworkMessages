using Unity.Netcode;

namespace UnityNetMessages.Events;

public class BetterMessage<T> : MessageReceiver
{
    protected override uint? GetHash()
    {
        throw new NotImplementedException();
    }

    protected override void OnReceiveMessage(ulong senderId, FastBufferReader buffer)
    {
        throw new NotImplementedException();
    }
}