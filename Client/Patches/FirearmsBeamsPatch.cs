using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using BetterTacticalDevicesHandling.Client.Devices;

namespace BetterTacticalDevicesHandling.Client.Patches;

// Every light change (own player, Fika's observed players) ends in Firearms.UpdateBeams.
public class FirearmsBeamsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() =>
        AccessTools.Method(typeof(Firearms), nameof(Firearms.UpdateBeams));

    [PatchPostfix]
    public static void PatchPostfix(Firearms __instance)
    {
        MissedBeams.Refresh(__instance);
    }
}
