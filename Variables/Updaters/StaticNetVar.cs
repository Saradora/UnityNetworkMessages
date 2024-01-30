using System.Reflection;
using OdinSerializer;
using Unity.Netcode;
using UnityNetMessages.Events;
using UnityNetMessages.OdinSerializer;

namespace UnityNetMessages.Variables;

public class VariableRequest
{
    
}

public class StaticNetVar<TObj, TVar> where TObj : NetworkBehaviour
{
    private Type ObjectType => typeof(TObj);
    private Type FieldType => typeof(TVar);
    private readonly FieldInfo _fieldInfo;

    private BindingFlags _privateFlags => BindingFlags.Static | BindingFlags.NonPublic;
    private BindingFlags _publicFlags => BindingFlags.Static | BindingFlags.Public;
    
    public StaticNetVar(string variableName, bool isPublic = true)
    {
        BindingFlags searchFlags = isPublic ? _publicFlags : _privateFlags;
        FieldInfo info = ObjectType.GetField(variableName, searchFlags);
        if (info is null)
        {
            info = ObjectType.GetField($"<{variableName}>k__BackingField", searchFlags);
            if (info is null)
                throw new ArgumentException($"There is no member named {variableName} in {ObjectType.Name}");
        }

        if (info.FieldType != FieldType)
            throw new ArgumentException($"Variable {variableName} is not of type {FieldType} ({info.FieldType})");

        _fieldInfo = info;
    }

    [Serializable]
    public struct StaticVariableData
    {
        public RuntimeTypeHandle Type;
        public string VariableName;
    }

    public VariableRequest ReadValue()
    {
        StaticVariableData varData = new() { Type = ObjectType.TypeHandle, VariableName = _fieldInfo.Name };
        byte[] bytes = SerializationUtility.SerializeValue(varData, DataFormat.Binary);

        FastBufferWriter writer = NetworkMessaging.GetWriter(EMessageType.StaticVariableRequest, bytes.SizeOf());
    }
}