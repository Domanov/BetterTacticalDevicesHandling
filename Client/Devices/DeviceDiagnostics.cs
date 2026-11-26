using System.Collections.Generic;
using System.Text;
using EFT;
using UnityEngine;

namespace BetterTacticalDevicesHandling.Client.Devices;

// Debug log for issue reports: which devices the game's own beam refresh would miss, and why.
internal static class DeviceDiagnostics
{
    public static void Log(Firearms firearms, List<TacticalDevice> devices)
    {
        List<TacticalComboVisualController> missing = MissedBeams.Missing(firearms);
        int listed = firearms._tacticalComboVisualControllers?.Length ?? 0;
        Plugin.Log.LogInfo($"Diagnostics: {devices.Count} device(s) on the weapon, {listed} refreshed by the game, {missing.Count} missed");
        foreach (TacticalComboVisualController visual in missing)
        {
            Plugin.Log.LogInfo(
                $"Diagnostics: missed {visual.LightMod.Item.ShortName.Localized(EStringCase.None)} ({visual.LightMod.Item.Id}), "
                + $"active {visual.gameObject.activeInHierarchy}, inactive parents: {InactiveParents(visual.transform, firearms._weaponHierarchy)}");
        }
    }

    private static string InactiveParents(Transform t, Transform root)
    {
        StringBuilder sb = new StringBuilder();
        for (; t != null && t != root; t = t.parent)
        {
            if (!t.gameObject.activeSelf)
            {
                sb.Append(t.name).Append(' ');
            }
        }

        return sb.Length > 0 ? sb.ToString() : "none";
    }
}
