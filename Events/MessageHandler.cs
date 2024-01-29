using Unity.Netcode;
using UnityNetMessages.Logging;
using UnityNetMessages.OdinSerializer;

namespace UnityNetMessages.Events;

internal struct NetworkEvent {}

public class MessageHandler
{
    public Type ReturnType { get; }

    private readonly List<WeakReference> _actions = new();

    public bool IsEmpty
    {
        get
        {
            if (_actions.Count <= 0) return true;

            foreach (var weakReference in _actions)
            {
                if (weakReference.IsAlive) return false;
            }

            return true;
        }
    }

    private MessageHandler(MessageReceiver action, Type returnType)
    {
        ReturnType = returnType;
        _actions.Add(new (action));
    }

    public static MessageHandler Create<TReturnType>(MessageReceiver action)
    {
        return new MessageHandler(action, typeof(TReturnType));
    }

    public void Subscribe<TReturnType>(MessageReceiver action)
    {
        if (ReturnType != typeof(TReturnType))
            throw new ArgumentException($"Cannot create network event with type {typeof(TReturnType).Name} because an event with the same name already exists with a different type of {ReturnType.Name}.");

        foreach (var weakReference in _actions)
        {
            if (weakReference.Target == action) return;
        }
        
        _actions.Add(new (action));
    }

    public void Unsubscribe(MessageReceiver action)
    {
        foreach (var reference in _actions)
        {
            if (!reference.IsAlive) continue;
            if (reference.Target != action) continue;
            
            _actions.Remove(reference);
            break;
        }
    }

    public void Raise(ulong clientId, FastBufferReader buffer)
    {
        Log.Error($"Raising {clientId}");
        
        int bufferLength = 0;
        if (!buffer.TryBeginReadValue(bufferLength))
        {
            Log.Error($"Couldn't read buffer length");
            return;
        }
        
        buffer.ReadValue(out bufferLength);

        object data = null;

        if (bufferLength > 0)
        {
            if (!buffer.TryBeginRead(bufferLength))
            {
                Log.Error($"Couldn't read buffer");
                return;
            }
            
            byte[] bytes = new byte[bufferLength];
            buffer.ReadBytes(ref bytes, bufferLength);
            data = Serialization.Deserialize(bytes, ReturnType);
        }
        
        for (int index = _actions.Count - 1; index >= 0; index--)
        {
            var weakRef = _actions[index];
            if (!weakRef.IsAlive)
            {
                _actions.RemoveAt(index);
                continue;
            }
            
            Log.Error($"Actually raising {index}");
            ((MessageReceiver)weakRef.Target).Invoke(clientId, data);
        }
    }
}