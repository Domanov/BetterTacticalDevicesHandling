using System.Reflection;
using EFT;
using EFT.InputSystem;
using HarmonyLib;
using SPT.Reflection.Patching;
using BetterTacticalDevicesHandling.Client.Devices;

namespace BetterTacticalDevicesHandling.Client.Patches;

// Runs for every command the player sends, so the command is checked before anything else.
public class TacticalKeyPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() =>
        AccessTools.Method(typeof(FirearmHandsInputTranslator), nameof(FirearmHandsInputTranslator.TranslateCommand));

    [PatchPrefix]
    public static bool PatchPrefix(FirearmHandsInputTranslator __instance, ECommand command, ref InputNode.ETranslateResult __result)
    {
        if (command != ECommand.ToggleTacticalDevice || !Plugin.TacticalKeyRestores.Value || !TacticalKeySwitch.Toggle(__instance))
        {
            return true;
        }

        __result = InputNode.ETranslateResult.Ignore;
        return false;
    }
}
