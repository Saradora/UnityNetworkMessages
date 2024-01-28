using Unity.Netcode;

namespace UnityNetMessages.Events;

/// <summary>
/// Base class for all events with no return data.
/// </summary>
public abstract class NetEventBase : MessageReceiver
{
    /// <summary>
    /// Called when an event is received on this identifier, by any source.
    /// </summary>
    /// <param name="senderId">The Id of the message sender.</param>
    public event Action<ulong> EventReceived;
    /// <summary>
    /// Called when an event is received on this identifier, coming from a client.
    /// This will never be called on a client (unless the client is also the host).
    /// </summary>
    /// <param name="senderId">The Id of the message sender.</param>
    public event Action<ulong> EventReceivedFromClient;
    /// <summary>
    /// Called when an event is received on this identifier, coming from a client.
    /// This will never be called on a server (unless the server is also the host).
    /// </summary>
    public event Action EventReceivedFromServer;

    protected override void OnReceiveMessage(ulong senderId, FastBufferReader buffer)
    {
        EventReceived?.Invoke(senderId);
        if (senderId == NetworkMessaging.ServerClientId)
            EventReceivedFromServer?.Invoke();
        else
            EventReceivedFromClient?.Invoke(senderId);
    }

    private FastBufferWriter GetWriterAndHash(out uint outHash)
    {
        uint? hash = GetHash();
        if (hash is null)
            throw new NullReferenceException("Cannot send as it is uninitialized.");

        var writer = NetworkMessaging.GetWriter(hash.Value, 0);
        outHash = hash.Value;
        return writer;
    }

    /// <summary>
    /// Invoke the event to the Server.
    /// Can only be called from a Client.
    /// </summary>
    /// <param name="delivery">The reliability of the delivery.</param>
    public void InvokeToServer(NetworkDelivery delivery = NetworkDelivery.ReliableSequenced)
    {
        using var writer = GetWriterAndHash(out uint hash);
        NetworkMessaging.TrySendMessageToServer<NetworkEvent>(hash, writer, delivery);
    }
    
    /// <summary>
    /// Invokes the event on a specific Client.
    /// Can only be called from the Server.
    /// </summary>
    /// <param name="clientId">The client Id who'll receive the event.</param>
    /// <param name="delivery">The reliability of the delivery.</param>
    public void InvokeToClient(ulong clientId, NetworkDelivery delivery = NetworkDelivery.ReliableSequenced)
    {
        using var writer = GetWriterAndHash(out uint hash);
        NetworkMessaging.TrySendMessageToClient<NetworkEvent>(hash, clientId, writer, delivery);
    }
    
    /// <summary>
    /// Invokes the event on all Clients.
    /// Can only be called from the Server.
    /// </summary>
    /// <param name="includeHost">Whether or not to include the Host client in the delivery.</param>
    /// <param name="delivery">The reliability of the delivery.</param>
    public void InvokeToAllClients(bool includeHost = false, NetworkDelivery delivery = NetworkDelivery.ReliableSequenced)
    {
        using var writer = GetWriterAndHash(out uint hash);
        NetworkMessaging.TrySendMessageToAllClients<NetworkEvent>(hash, writer, includeHost, delivery);
    }
}