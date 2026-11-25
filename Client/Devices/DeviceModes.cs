using System;
using System.Collections.Generic;
using System.Text;
using EFT;
using EFT.InventoryLogic;
using EFT.Visual;
using HarmonyLib;
using UnityEngine;

namespace BetterTacticalDevicesHandling.Client.Devices;

// Templates only carry ModesCount, so the parts of each mode are read from its mode_NNN child on the weapon model.
// Rule checked against every vanilla device and the modded ones in the Fika install (docs/better-tactical-devices-handling-plan.md).
internal static class DeviceModes
{
    // Game's own (misspelt) field name.
    private const string ModeTransformsField = "_ligthbeamsTransforms";
    private const string IndicatorName = "indicator";
    private const string DumpIndent = "  ";
    private const string NoModeTransform = "(no mode transform)";

    private static readonly AccessTools.FieldRef<TacticalComboVisualController, List<Transform>> ModeTransforms =
        AccessTools.FieldRefAccess<TacticalComboVisualController, List<Transform>>(ModeTransformsField);

    // A template always uses the same prefab, so each device type is scanned once per session.
    private static readonly Dictionary<MongoID, DeviceParts[]> Cache = new Dictionary<MongoID, DeviceParts[]>();

    public static DeviceParts[] Cached(Item item) => Cache.TryGetValue(item.TemplateId, out DeviceParts[] parts) ? parts : null;

    public static DeviceParts[] Scan(LightComponent light, TacticalComboVisualController visual)
    {
        DeviceParts[] parts = new DeviceParts[Mathf.Max(1, light._template.ModesCount)];
        if (visual != null)
        {
            Cache[light.Item.TemplateId] = parts;
        }

        List<Transform> modes = visual != null ? ModeTransforms(visual) : null;
        for (int i = 0; i < parts.Length; i++)
        {
            Transform mode = modes != null && i < modes.Count ? modes[i] : null;
            parts[i] = mode != null ? Parts(mode) : DeviceParts.None;
            if (Plugin.LogModes.Value)
            {
                Plugin.Log.LogInfo($"{light.Item.ShortName.Localized(EStringCase.None)} mode {i}: {parts[i]}\n{Dump(mode)}");
            }
        }

        return parts;
    }

    private static DeviceParts Parts(Transform mode)
    {
        DeviceParts parts = DeviceParts.None;
        foreach (LaserBeam laser in mode.GetComponentsInChildren<LaserBeam>(true))
        {
            parts |= laser.GetComponent<IkLight>() != null ? DeviceParts.IrLaser : DeviceParts.VisLaser;
        }

        foreach (Light light in mode.GetComponentsInChildren<Light>(true))
        {
            // A laser's dot light (created at runtime under the beam) and the status LED (indicator_NNN) are lights too,
            // but not parts. Modes that aren't selected are inactive, so the lookup must include inactive objects.
            if (light.GetComponentInParent<LaserBeam>(true) != null || IsUnderIndicator(light.transform, mode))
            {
                continue;
            }

            parts |= light.GetComponent<IkLight>() != null ? DeviceParts.IrIlluminator : DeviceParts.Light;
        }

        return parts;
    }

    private static bool IsUnderIndicator(Transform t, Transform mode)
    {
        for (; t != null && t != mode; t = t.parent)
        {
            if (t.name.IndexOf(IndicatorName, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private static string Dump(Transform mode)
    {
        if (mode == null)
        {
            return DumpIndent + NoModeTransform;
        }

        StringBuilder sb = new StringBuilder();
        foreach (Transform t in mode.GetComponentsInChildren<Transform>(true))
        {
            sb.Append(DumpIndent).Append(t.name).Append(':');
            foreach (Component c in t.GetComponents<Component>())
            {
                sb.Append(' ').Append(c.GetType().Name);
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}
