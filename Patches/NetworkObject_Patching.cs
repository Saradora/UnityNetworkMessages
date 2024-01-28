using HarmonyLib;
using Unity.Netcode;

namespace UnityNetMessages.Patches;

[HarmonyPatch(typeof(NetworkObject))]
public static class NetworkObject_Patching
{
    private static Dictionary<NetworkObject, Action<ulong>> _networkIdsEvents = new();

    [HarmonyPatch("NetworkObjectId", MethodType.Setter), HarmonyPostfix]
    private static void NetworkObjectId_SetterPostfix(ulong value, NetworkObject __instance)
    {
        if (!_networkIdsEvents.ContainsKey(__instance))
            return;
        
        _networkIdsEvents[__instance]?.Invoke(value);
    }

    public static void RegisterToObjectIdChanges(this NetworkObject obj, Action<ulong> action)
    {
        if (!_networkIdsEvents.ContainsKey(obj)) _networkIdsEvents.Add(obj, action);
        else _networkIdsEvents[obj] += action;
    }

    public static void UnregisterFromObjectIdChanges(this NetworkObject obj, Action<ulong> action)
    {
        if (!_networkIdsEvents.ContainsKey(obj)) return;
        _networkIdsEvents[obj] -= action;
        if (_networkIdsEvents[obj] is null)
        {
            _networkIdsEvents.Remove(obj);
        }
    }

    [HarmonyPatch("OnDestroy"), HarmonyPrefix]
    private static void OnDestroy_Prefix(NetworkObject __instance)
    {
        if (!_networkIdsEvents.ContainsKey(__instance)) return;
        
        _networkIdsEvents[__instance]?.Invoke(0);
        _networkIdsEvents.Remove(__instance);
    }
}