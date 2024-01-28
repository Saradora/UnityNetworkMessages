using System.Diagnostics;
using System.Reflection;
using UnityNetMessages.Logging;

namespace UnityNetMessages.Events;

public abstract class GlobalNetMessageBase<T> : NetMessageBase<T>
{
    private readonly uint _hash;

    internal GlobalNetMessageBase(string name, bool assemblySpecific = false)
    {
        string messageName = name;
        if (assemblySpecific) messageName = Assembly.GetCallingAssembly().GetName().Name + "+" + messageName;
        _hash = messageName.Hash32();
        
        RegisterEvent<T>();
    }

    protected override uint? GetHash() => _hash;
}