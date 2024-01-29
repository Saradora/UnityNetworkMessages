using Unity.Netcode;
using UnityNetMessages.Patches;

namespace UnityNetMessages.Events;
public class ObjectMessageLink
{
    public uint? Hash { get; private set; }
    private readonly string _baseName;
    private readonly NetworkObject _targetObject;
    private readonly MessageReceiver _messageReceiver;
    
    public ObjectMessageLink(string baseName, NetworkObject obj, MessageReceiver receiver)
    {
        Hash = null;
        _baseName = baseName;
        _targetObject = obj;
        _messageReceiver = receiver;
        RegisterNetworkObject();
        _targetObject.RegisterToObjectIdChanges(OnNetworkObjectIdChanged);
    }

    private void OnNetworkObjectIdChanged(ulong value)
    {
        if (value != 0)
        {
            RegisterNetworkObject();
        }
        else
        {
            Dispose();
        }
    }
    
    private void RegisterNetworkObject()
    {
        if (_targetObject.NetworkObjectId == 0) return;
        if (Hash.HasValue)
        {
            UnregisterNetworkObject();
        }

        Hash = $"obj{_targetObject.NetworkObjectId}+{_baseName}".Hash32();
        _messageReceiver.RegisterEvent<NetworkEvent>();
    }

    private void UnregisterNetworkObject()
    {
        if (!Hash.HasValue) return;
        _messageReceiver.UnregisterEvent();
        Hash = null;
    }

    public void Dispose()
    {
        UnregisterNetworkObject();
        if (_targetObject != null)
        {
            _targetObject.UnregisterFromObjectIdChanges(OnNetworkObjectIdChanged);
        }
    }
}