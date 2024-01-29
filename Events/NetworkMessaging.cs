using Unity.Collections;
using Unity.Netcode;
using UnityNetMessages.Logging;

namespace UnityNetMessages.Events;

public enum EMessageType : byte
{
    Event = 0,
    Data = 1,
}

public static class NetworkMessaging
{
    private static readonly Dictionary<uint, MessageHandler> _registeredMessages = new();
    private static CustomMessagingManager _messenger;

    public static bool IsServer => NetworkManager.Singleton.IsServer;

    public static ulong ServerClientId => NetworkManager.ServerClientId;

    public static event Action<NetworkManager> SingletonChanged;
    public static event Action<CustomMessagingManager> MessagingManagerChanged;

    internal static void TriggerSingletonChange(NetworkManager singleton) => 
        SingletonChanged?.Invoke(singleton);

    internal static void TriggerMessengerChange(CustomMessagingManager messenger) =>
        MessagingManagerChanged?.Invoke(messenger);

    private static readonly List<ulong> _clientIds = new();

    public static IReadOnlyList<ulong> NonHostClientIds
    {
        get
        {
            var ids = NetworkManager.Singleton.ConnectedClientsIds;
            _clientIds.Clear();
            ulong serverId = NetworkManager.ServerClientId;
            foreach (var clientId in ids)
            {
                if (clientId == serverId)
                {
                    continue;
                }
                _clientIds.Add(clientId);
            }
            return _clientIds;
        }
    }

    public static FastBufferWriter GetWriter(EMessageType messageType, uint hash, int size)
    {
        FastBufferWriter writer;
        switch (messageType)
        {
            case EMessageType.Event:
                writer = new FastBufferWriter(sizeof(byte) + sizeof(uint), Allocator.Temp);
                writer.WriteValue(messageType);
                writer.WriteValue(hash);
                break;
            case EMessageType.Data:
                writer = new FastBufferWriter(sizeof(byte) + sizeof(uint) + size, Allocator.Temp);
                writer.WriteValue(messageType);
                writer.WriteValue(hash);
                break;
            default:
                writer = new FastBufferWriter(sizeof(byte), Allocator.Temp);
                break;
        }
        return writer;
    }

    static NetworkMessaging()
    {
        MessagingManagerChanged -= OnMessagingManagerChanged;
        MessagingManagerChanged += OnMessagingManagerChanged;
    }

    private static void OnMessagingManagerChanged(CustomMessagingManager instance)
    {
        if (_messenger is not null)
        {
            _messenger.OnUnnamedMessage -= OnUnnamedMessageReceived;
        }

        _messenger = instance;

        if (_messenger is null) return;
        
        _messenger.OnUnnamedMessage += OnUnnamedMessageReceived;
    }

    private static void OnUnnamedMessageReceived(ulong clientId, FastBufferReader bufferReader)
    {
        EMessageType messageType = EMessageType.Event;
        if (!bufferReader.TryBeginReadValue(messageType))
            return;
        
        bufferReader.ReadValue(out messageType);

        uint hash = 0;
        MessageHandler handler;
        switch (messageType)
        {
            case EMessageType.Event:
                if (!bufferReader.TryBeginReadValue(hash))
                    return;

                bufferReader.ReadValue(out hash);
                
                if (_registeredMessages.TryGetValue(hash, out handler))
                {
                    handler.RaiseEvent(clientId);
                }

                break;
            case EMessageType.Data:
                if (!bufferReader.TryBeginReadValue(hash))
                    return;

                bufferReader.ReadValue(out hash);
                
                if (_registeredMessages.TryGetValue(hash, out handler))
                {
                    handler.RaiseMessage(clientId, bufferReader);
                }
                break;
            default:
                break;
        }
        
        //bufferReader.Seek(FastBufferWriter.GetWriteSize<uint>());
    }

    public static void RegisterEvent<TReturnType>(uint hash, MessageReceiver action)
    {
        if (!_registeredMessages.ContainsKey(hash))
        {
            _registeredMessages[hash] = MessageHandler.Create<TReturnType>(action);
        }
        else
        {
            _registeredMessages[hash].Subscribe<TReturnType>(action);
        }
    }

    public static void UnregisterEvent(uint hash, MessageReceiver action)
    {
        if (!_registeredMessages.ContainsKey(hash)) return;

        MessageHandler handler = _registeredMessages[hash];

        handler.Unsubscribe(action);

        if (handler.IsEmpty)
        {
            _registeredMessages.Remove(hash);
        }
    }

    public static void TrySendMessageToServer<TReturnType>(uint hash, FastBufferWriter writer, NetworkDelivery delivery = NetworkDelivery.ReliableSequenced)
    {
        if (IsServer)
        {
            Log.Warning($"Message couldn't be sent because you are the server.");
            return;
        }
        
        TrySendMessageToClientInternal<TReturnType>(hash, ServerClientId, writer, delivery);
    }

    public static void TrySendMessageToClient<TReturnType>(uint hash, ulong clientId, FastBufferWriter writer, NetworkDelivery delivery = NetworkDelivery.ReliableSequenced)
    {
        if (!IsServer)
        {
            Log.Warning($"Message couldn't be sent because you are not the server.");
            return;
        }
        
        TrySendMessageToClientInternal<TReturnType>(hash, clientId, writer, delivery);
    }

    public static void TrySendMessageToAllClients<TReturnType>(uint hash, FastBufferWriter writer, bool includeHost, NetworkDelivery delivery = NetworkDelivery.ReliableSequenced)
    {
        if (!IsServer)
        {
            Log.Warning($"Message couldn't be sent because you are not the server.");
            return;
        }

        if (!ValidateHandler<TReturnType>(hash))
            return;
        
        if (includeHost)
            _messenger.SendUnnamedMessageToAll(writer, delivery);
        else
            _messenger.SendUnnamedMessage(NonHostClientIds, writer, delivery);
    }

    private static void TrySendMessageToClientInternal<TReturnType>(uint hash, ulong clientId, FastBufferWriter writer, NetworkDelivery delivery)
    {
        if (!ValidateHandler<TReturnType>(hash))
            return;

        _messenger.SendUnnamedMessage(clientId, writer, delivery);
    }

    private static bool ValidateHandler<TReturnType>(uint hash)
    {
        if (_messenger is null)
        {
            Log.Warning($"Message couldn't be sent because messenger isn't active");
            return false;
        }
        
        if (!_registeredMessages.TryGetValue(hash, out MessageHandler handler))
        {
            Log.Warning($"Message couldn't be sent because it wasn't registered to the messenger");
            return false;
        }

        if (handler.ReturnType != typeof(TReturnType))
        {
            Log.Error($"Message couldn't be sent because the registered event with the same name has a different return type.");
            return false;
        }

        return true;
    }
}