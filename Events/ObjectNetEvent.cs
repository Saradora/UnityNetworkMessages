using System.Reflection;
using Unity.Netcode;
using UnityNetMessages.Patches;

namespace UnityNetMessages.Events;

/// <summary>
/// Wrapper for an event (with no arguments) relative to a NetworkObject.
/// </summary>
public class ObjectNetEvent : NetEventBase
{
    private readonly ObjectMessageLink _objectMessageLink;

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
        
        if (assemblySpecific) name = Assembly.GetCallingAssembly().GetName().Name + "+" + name;

        _objectMessageLink = new ObjectMessageLink(name, targetObject, this);
    }

    protected override uint? GetHash()
    {
        return _objectMessageLink.Hash;
    }

    protected override void DoDispose()
    {
        _objectMessageLink.Dispose();
    }
}