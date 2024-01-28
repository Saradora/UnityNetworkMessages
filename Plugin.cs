using BepInEx;
using HarmonyLib;

namespace UnityNetMessages;

[BepInPlugin(UnityNetworkMessages.ModGuid, UnityNetworkMessages.ModName, UnityNetworkMessages.ModVersion)]
public class Plugin : BaseUnityPlugin
{
    private readonly Harmony _harmonyInstance = new(UnityNetworkMessages.ModGuid);

    private void Awake()
    {
        _harmonyInstance.PatchAll();
    }
}