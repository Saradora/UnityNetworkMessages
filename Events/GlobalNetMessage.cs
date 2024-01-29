using System.Reflection;

namespace UnityNetMessages.Events;

public class GlobalNetMessage<T> : NetMessageBase<T>
{
    private readonly uint _hash;

    public GlobalNetMessage(string name, bool assemblySpecific = false)
    {
        string messageName = name;
        if (assemblySpecific) messageName = Assembly.GetCallingAssembly().GetName().Name + "+" + messageName;
        _hash = messageName.Hash32();
        
        RegisterEvent<T>();
    }

    protected override uint? GetHash() => _hash;
}