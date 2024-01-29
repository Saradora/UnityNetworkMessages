using System.Reflection;
using Unity.Netcode;

namespace UnityNetMessages.Events;

public class ObjectNetMessage<T> : NetMessageBase<T>
{
    private readonly ObjectMessageLink<T> _objectMessageLink;

    public ObjectNetMessage(string name, NetworkObject targetObject, bool assemblySpecific = false)
    {
        if (targetObject == null)
            throw new NullReferenceException("Cannot create an object event without a target object.");

        if (assemblySpecific) name = Assembly.GetCallingAssembly().GetName().Name + "+" + name;
        _objectMessageLink = new ObjectMessageLink<T>(name, targetObject, this);
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