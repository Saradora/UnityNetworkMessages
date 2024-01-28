using System.Reflection;
using Unity.Netcode;
using UnityNetMessages.Patches;
using UnityNetMessaging;

namespace UnityNetMessages.Events;

/// <summary>
/// Wrapper for an event (with no arguments) relative to a NetworkObject.
/// </summary>
public class ObjectNetEvent : NetEventBase
{
    private uint? _hash;
    private readonly string _baseName;
    private readonly NetworkObject _targetObject;

    /// <summary>
    /// Wrapper for an event (with no arguments) relative to a NetworkObject.
    /// </summary>
    /// <param name="name">The static identifier to connect the event.</param>
    /// <param name="targetObject">The target NetworkObject.</param>
    /// <param name="assemblySpecific">Whether to accept sending/receiving messages with this identifier from other assemblies.</param>
    public ObjectNetEvent(string name, NetworkObject targetObject, bool assemblySpecific = false)
    {
        if (targetObject == null)
            throw new NullReferenceException("Cannot create an object event without a target object.");
        
        _baseName = name;
        if (assemblySpecific) _baseName = Assembly.GetCallingAssembly().GetName().Name + "+" + _baseName;
        _targetObject = targetObject;
        _targetObject.RegisterToObjectIdChanges(OnNetworkObjectIdChanged);
        RegisterNetworkObject();
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

    protected override uint? GetHash()
    {
        return _hash;
    }
    
    private void RegisterNetworkObject()
    {
        if (_targetObject.NetworkObjectId == 0) return;
        if (_hash.HasValue)
        {
            UnregisterNetworkObject();
        }

        _hash = $"obj{_targetObject.NetworkObjectId}+{_baseName}".Hash32();
        RegisterEvent<Unity.Netcode.NetworkEvent>();
    }

    private void UnregisterNetworkObject()
    {
        if (!_hash.HasValue) return;
        UnregisterEvent();
        _hash = null;
    }

    protected override void DoDispose()
    {
        UnregisterNetworkObject();
        if (_targetObject != null)
        {
            _targetObject.UnregisterFromObjectIdChanges(OnNetworkObjectIdChanged);
        }
    }
}