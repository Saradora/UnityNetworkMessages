using System.Reflection;

namespace UnityNetMessages.Events;

/// <summary>
/// Wrapper for an event (with no arguments) with a unique static identifier.
/// </summary>
public class GlobalNetEvent : NetEventBase
{
    private readonly uint _hash;

    /// <summary>
    /// Wrapper for an event (with no arguments) with a unique static identifier.
    /// </summary>
    /// <param name="name">The static identifier to connect the event.</param>
    /// <param name="assemblySpecific">Whether to accept sending/receiving messages with this identifier from other assemblies.</param>
    public GlobalNetEvent(string name, bool assemblySpecific = false)
    {
        string messageName = name;
        if (assemblySpecific) messageName = Assembly.GetCallingAssembly().GetName().Name + "+" + messageName;
        _hash = messageName.Hash32();
        
        RegisterEvent<NetworkEvent>();
    }

    protected override uint? GetHash()
    {
        return _hash;
    }
}