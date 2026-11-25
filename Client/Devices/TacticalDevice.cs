using System.Collections.Generic;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

namespace BetterTacticalDevicesHandling.Client.Devices;

internal sealed class TacticalDevice
{
    public readonly LightComponent Light;
    public readonly string Name;
    public readonly DeviceParts[] ModeParts;
    public readonly DeviceParts AllParts;
    public readonly bool HasVariants;

    private TacticalDevice(LightComponent light, DeviceParts[] modeParts)
    {
        Light = light;
        Name = light.Item.ShortName.Localized(EStringCase.None);
        ModeParts = modeParts;
        for (int i = 0; i < modeParts.Length; i++)
        {
            AllParts |= modeParts[i];
            HasVariants |= modeParts[i] != DeviceParts.None && ModePicker.VariantCount(modeParts, i) > 1;
        }
    }

    public int Mode => Light.SelectedMode % ModeParts.Length;

    public DeviceParts ActiveParts => Light.IsActive ? ModeParts[Mode] : DeviceParts.None;

    public int VariantCount => ModePicker.VariantCount(ModeParts, Mode);

    public int VariantNumber => ModePicker.VariantIndex(ModeParts, Mode) + 1;

    public LightsState TogglePart(DeviceParts part) => State(ModePicker.TogglePart(ModeParts, Light.IsActive, Mode, part));

    public LightsState NextVariant() => State(ModePicker.NextVariant(ModeParts, Mode));

    public LightsState TogglePower() => Light.GetLightState(true, false);

    public LightsState Current() => Light.GetLightState(false, false);

    public LightsState TurnedOff() => State(ModePicker.Off);

    public LightsState NextMode() => Light.GetLightState(false, true);

    private LightsState State(int mode) => new LightsState
    {
        Id = Light.Item.Id,
        IsActive = mode != ModePicker.Off,
        LightMode = mode != ModePicker.Off ? mode : Light.SelectedMode
    };

    // Same device set and order as FirearmHandsInputTranslator.GetCurrentLightStates.
    // The model (hands or preview) is only searched when a device type isn't cached yet.
    public static List<TacticalDevice> Collect(CompoundItem weapon, Transform model)
    {
        TacticalComboVisualController[] visuals = null;
        List<TacticalDevice> devices = new List<TacticalDevice>();
        foreach (Slot slot in weapon.AllSlots)
        {
            Item item = slot.ContainedItem;
            if (item == null || !item.TryGetItemComponent(out LightComponent light))
            {
                continue;
            }

            DeviceParts[] parts = DeviceModes.Cached(item);
            if (parts == null)
            {
                visuals ??= model.GetComponentsInChildren<TacticalComboVisualController>(true);
                parts = DeviceModes.Scan(light, FindVisual(visuals, item.Id));
            }

            devices.Add(new TacticalDevice(light, parts));
        }

        return devices;
    }

    public static TacticalComboVisualController FindVisual(TacticalComboVisualController[] visuals, string itemId)
    {
        foreach (TacticalComboVisualController visual in visuals)
        {
            if (visual.LightMod != null && visual.LightMod.Item.Id == itemId)
            {
                return visual;
            }
        }

        return null;
    }
}
