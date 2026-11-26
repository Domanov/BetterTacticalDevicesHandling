using System;
using System.Collections.Generic;
using System.Text;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

namespace BetterTacticalDevicesHandling.Client.Devices;

// Debug log for issue reports: for every device on the weapon, each beam object on the model, whether the game
// refreshes it, whether it can draw, which item instance it reads and which mode it shows.
internal static class DeviceDiagnostics
{
    public static void Log(Firearms firearms, CompoundItem weapon, string trigger)
    {
        TacticalComboVisualController[] listed = firearms._tacticalComboVisualControllers;
        TacticalComboVisualController[] visuals = firearms._weaponHierarchy != null
            ? firearms._weaponHierarchy.GetComponentsInChildren<TacticalComboVisualController>(true)
            : Array.Empty<TacticalComboVisualController>();
        Plugin.Log.LogInfo(
            $"Diagnostics ({trigger}): {Name(weapon)} ({weapon.Id}), {listed?.Length ?? 0} beam object(s) refreshed by the game, {visuals.Length} on the model");

        HashSet<string> deviceIds = new HashSet<string>();
        foreach (Slot slot in weapon.AllSlots)
        {
            Item item = slot.ContainedItem;
            if (item == null || !item.TryGetItemComponent(out LightComponent light))
            {
                continue;
            }

            deviceIds.Add(item.Id);
            Plugin.Log.LogInfo($"Diagnostics: {Name(item)} ({item.Id}) in {slot.ID}: {State(light)}");
            int found = 0;
            foreach (TacticalComboVisualController visual in visuals)
            {
                if (visual.LightMod != null && visual.LightMod.Item.Id == item.Id)
                {
                    found++;
                    LogVisual(visual, light, listed, firearms._weaponHierarchy);
                }
            }

            if (found == 0)
            {
                Plugin.Log.LogInfo("Diagnostics:   no beam object on the model");
            }
        }

        foreach (TacticalComboVisualController visual in visuals)
        {
            if (visual.LightMod == null || !deviceIds.Contains(visual.LightMod.Item.Id))
            {
                string reads = visual.LightMod != null ? $"{Name(visual.LightMod.Item)} ({visual.LightMod.Item.Id})" : "nothing";
                Plugin.Log.LogInfo($"Diagnostics: beam object {visual.name} reads {reads}, which is not on the weapon");
            }
        }
    }

    private static void LogVisual(TacticalComboVisualController visual, LightComponent light, TacticalComboVisualController[] listed, Transform root)
    {
        bool refreshed = listed != null && Array.IndexOf(listed, visual) >= 0;
        bool sameItem = ReferenceEquals(visual.LightMod, light);
        Plugin.Log.LogInfo(
            $"Diagnostics:   beam object {visual.name}: refreshed by game {refreshed}, active {visual.gameObject.activeInHierarchy}, "
            + $"inactive parents: {InactiveParents(visual.transform, root)}, same item instance {sameItem}"
            + (sameItem ? string.Empty : $" (reads {State(visual.LightMod)})")
            + $", showing {ShownModes(visual)}");
    }

    private static string State(LightComponent light) => $"{(light.IsActive ? "on" : "off")}, mode {light.SelectedMode}";

    private static string ShownModes(TacticalComboVisualController visual)
    {
        List<Transform> modes = DeviceModes.ModeTransforms(visual);
        if (modes == null)
        {
            return "nothing (no mode list)";
        }

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < modes.Count; i++)
        {
            if (modes[i] != null && modes[i].gameObject.activeSelf)
            {
                sb.Append(modes[i].name).Append(' ');
            }
        }

        return sb.Length > 0 ? sb.ToString().TrimEnd() : "no mode";
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

        return sb.Length > 0 ? sb.ToString().TrimEnd() : "none";
    }

    private static string Name(Item item) => item.ShortName.Localized(EStringCase.None);
}
