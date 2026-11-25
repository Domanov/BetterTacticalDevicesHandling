using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.WeaponModding;
using HarmonyLib;
using SPT.Reflection.Patching;
using BetterTacticalDevicesHandling.Client.Inspect;

namespace BetterTacticalDevicesHandling.Client.Patches;

public class InspectShowPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() =>
        AccessTools.Method(typeof(ItemSpecificationPanel), nameof(ItemSpecificationPanel.Show));

    [PatchPostfix]
    public static void PatchPostfix(ItemSpecificationPanel __instance, ItemContext itemContext, WeaponPreview ____weaponPreview)
    {
        if (itemContext.Item is Weapon weapon && ____weaponPreview != null)
        {
            InspectPanel.Attach(__instance._itemLabels._previewPanel, ____weaponPreview, weapon);
        }
    }
}

// Before the vanilla Close returns the preview to its pool.
public class InspectClosePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() =>
        AccessTools.Method(typeof(ItemSpecificationPanel), nameof(ItemSpecificationPanel.Close));

    [PatchPrefix]
    public static void PatchPrefix(WeaponPreview ____weaponPreview)
    {
        InspectPanel.Detach(____weaponPreview);
    }
}

// Runs right after the preview model is built and positioned.
public class PreviewBuiltPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() =>
        AccessTools.Method(typeof(WeaponPreview), nameof(WeaponPreview.SetPreviewMaskToChildrenLights));

    [PatchPostfix]
    public static void PatchPostfix(WeaponPreview __instance)
    {
        InspectPanel.OnModelBuilt(__instance);
    }
}
