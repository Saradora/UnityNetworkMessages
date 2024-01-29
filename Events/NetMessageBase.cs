using OdinSerializer;
using Unity.Netcode;
using UnityNetMessages.Logging;

namespace UnityNetMessages.Events;

/// <summary>
/// Base class for all events that send data.
/// </summary>
/// <typeparam name="T">The data type.</typeparam>
public abstract class NetMessageBase<T> : MessageReceiver
{
    /// <summary>
    /// Called when a message is received on this identifier, by any source.
    /// </summary>
    /// <param name="data">The data received.</param>
    /// <param name="senderId">The Id of the message sender.</param>
    public event Action<T, ulong> MessageReceived;
    /// <summary>
    /// Called when a message is received on this identifier, coming from a client.
    /// This will never be called on a client (unless the client is also the host).
    /// </summary>
    /// <param name="data">The data received.</param>
    /// <param name="senderId">The Id of the message sender.</param>
    public event Action<T, ulong> MessageReceivedFromClient;
    /// <summary>
    /// Called when a message is received on this identifier, coming from a client.
    /// This will never be called on a server (unless the server is also the host).
    /// </summary>
    /// <param name="data">The data received.</param>
    public event Action<T> MessageReceivedFromServer;

    protected override void OnReceiveMessage(ulong senderId, FastBufferReader buffer)
    {
        if (!TryReadBuffer(buffer, out T value))
            return;
        
        MessageReceived?.Invoke(value, senderId);
        if (senderId == NetworkMessaging.ServerClientId)
            MessageReceivedFromServer?.Invoke(value);
        else
            MessageReceivedFromClient?.Invoke(value, senderId);
    }

    protected abstract bool TryReadBuffer(FastBufferReader buffer, out T outValue);

    protected abstract int GetWriteSize(T data);

    protected abstract void WriteToBuffer(T data, FastBufferWriter writer);

    private FastBufferWriter GetWriterAndHash(T data, out uint outHash)
    {
        uint? hash = GetHash();
        if (hash is null)
            throw new NullReferenceException("Cannot send as it is uninitialized.");

        var writer = NetworkMessaging.GetWriter(hash.Value, GetWriteSize(data));
        WriteToBuffer(data, writer);
        outHash = hash.Value;
        return writer;
    }

    /// <summary>
    /// Sends data to the Server.
    /// Can only be called from a Client.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="delivery">The reliability of the delivery.</param>
    public void SendToServer(T data, NetworkDelivery delivery = NetworkDelivery.ReliableSequenced)
    {
        using var writer = GetWriterAndHash(data, out var hash);
        try
        {
            NetworkMessaging.TrySendMessageToServer<T>(hash, writer, delivery);
        }
        catch (Exception e)
        {
            Log.Exception(e);
        }
    }

    /// <summary>
    /// Sends data to a specific Client.
    /// Can only be called from the Server.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="clientId">The client Id who'll receive the data.</param>
    /// <param name="delivery">The reliability of the delivery.</param>
    public void SendToClient(T data, ulong clientId, NetworkDelivery delivery = NetworkDelivery.ReliableSequenced)
    {
        using var writer = GetWriterAndHash(data, out var hash);
        try
        {
            NetworkMessaging.TrySendMessageToClient<T>(hash, clientId, writer, delivery);
        }
        catch (Exception e)
        {
            Log.Exception(e);
        }
    }

    /// <summary>
    /// Sends data to all Clients.
    /// Can only be called from the Server.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="includeHost">Whether or not to include the Host client in the delivery.</param>
    /// <param name="delivery">The reliability of the delivery.</param>
    public void SendToAllClients(T data, bool includeHost = false, NetworkDelivery delivery = NetworkDelivery.ReliableSequenced)
    {
        using var writer = GetWriterAndHash(data, out var hash);
        try
        {
            NetworkMessaging.TrySendMessageToAllClients<T>(hash, writer, includeHost, delivery);
        }
        catch (Exception e)
        {
            Log.Exception(e);
        }
    }
}