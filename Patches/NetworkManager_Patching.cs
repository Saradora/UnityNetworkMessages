using HarmonyLib;
using Unity.Netcode;
using UnityNetMessages.Events;

namespace UnityNetMessages.Patches;

[HarmonyPatch(typeof(NetworkManager))]
public static class NetworkManager_Patching
{
    private static NetworkManager _currentSingleton;
    private static CustomMessagingManager _currentMessagingManager;
    
    [HarmonyPatch("Singleton", MethodType.Setter), HarmonyPostfix]
    private static void Singleton_SetterPostfix(NetworkManager value)
    {
        if (_currentSingleton == value) return;
        if (value == null) CustomMessagingManager_SetterPostfix(null);
        NetworkMessaging.TriggerSingletonChange(value);
        _currentSingleton = value;
    }

    [HarmonyPatch("CustomMessagingManager", MethodType.Setter), HarmonyPostfix]
    private static void CustomMessagingManager_SetterPostfix(CustomMessagingManager value)
    {
        if (_currentMessagingManager == value) return;
        NetworkMessaging.TriggerMessengerChange(value);
        _currentMessagingManager = value;
    }
}