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

    protected override void OnReceiveMessage(ulong senderId, object data)
    {
        if (data is not T castData)
            return;
        
        MessageReceived?.Invoke(castData, senderId);
        if (senderId == NetworkMessaging.ServerClientId)
            MessageReceivedFromServer?.Invoke(castData);
        else
            MessageReceivedFromClient?.Invoke(castData, senderId);
    }

    private bool TryGetWriterAndHash(T data, out uint outHash, out FastBufferWriter outWriter)
    {
        uint? hash = GetHash();
        if (hash is null)
            throw new NullReferenceException("Cannot send as it is uninitialized.");

        byte[] bytes;
        try
        {
            bytes = SerializationUtility.SerializeValue(data, DataFormat.Binary);
        }
        catch
        {
            Log.Exception(new Exception($"Cannot serialize type {data.GetType().Name}"));
            throw;
        }
        
        outWriter = NetworkMessaging.GetWriter(EMessageType.Data, hash.Value, sizeof(byte) * bytes.Length);
        outWriter.WriteBytes(bytes);
        outHash = hash.Value;
        return true;
    }

    /// <summary>
    /// Sends data to the Server.
    /// Can only be called from a Client.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="delivery">The reliability of the delivery.</param>
    public void SendToServer(T data, NetworkDelivery delivery = NetworkDelivery.ReliableSequenced)
    {
        if (!TryGetWriterAndHash(data, out var hash, out var writer))
            return;

        using (writer)
        {
            try
            {
                NetworkMessaging.TrySendMessageToServer<T>(hash, writer, delivery);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
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
        if (!TryGetWriterAndHash(data, out var hash, out var writer))
            return;
        
        using (writer)
        {
            try
            {
                NetworkMessaging.TrySendMessageToClient<T>(hash, clientId, writer, delivery);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
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
        if (!TryGetWriterAndHash(data, out var hash, out var writer))
            return;

        using (writer)
        {
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
}