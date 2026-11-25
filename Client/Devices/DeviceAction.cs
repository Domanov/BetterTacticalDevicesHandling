using System.Collections.Generic;
using EFT;

namespace BetterTacticalDevicesHandling.Client.Devices;

internal enum DeviceActionKind
{
    Part,
    Variant,
    Power,
    NextMode
}

internal readonly struct DeviceAction
{
    private static readonly DeviceParts[] PartOrder =
        { DeviceParts.VisLaser, DeviceParts.IrLaser, DeviceParts.Light, DeviceParts.IrIlluminator };

    public readonly TacticalDevice Device;
    public readonly DeviceActionKind Kind;
    public readonly DeviceParts Part;

    private DeviceAction(TacticalDevice device, DeviceActionKind kind, DeviceParts part = DeviceParts.None)
    {
        Device = device;
        Kind = kind;
        Part = part;
    }

    // Devices whose modes show no recognised parts keep plain on/off + mode cycling.
    public static void AddFor(TacticalDevice device, List<DeviceAction> actions)
    {
        if (device.AllParts == DeviceParts.None)
        {
            actions.Add(new DeviceAction(device, DeviceActionKind.Power));
            if (device.ModeParts.Length > 1)
            {
                actions.Add(new DeviceAction(device, DeviceActionKind.NextMode));
            }

            return;
        }

        foreach (DeviceParts part in PartOrder)
        {
            if ((device.AllParts & part) != 0)
            {
                actions.Add(new DeviceAction(device, DeviceActionKind.Part, part));
            }
        }

        if (device.HasVariants)
        {
            actions.Add(new DeviceAction(device, DeviceActionKind.Variant));
        }
    }

    public bool IsLit => Kind switch
    {
        DeviceActionKind.Part => (Device.ActiveParts & Part) != 0,
        DeviceActionKind.Variant => CanSwitchVariant,
        DeviceActionKind.Power => Device.Light.IsActive,
        _ => true
    };

    public string Title => Kind switch
    {
        DeviceActionKind.Part => PartName(Part),
        DeviceActionKind.Variant => "VARIANT",
        DeviceActionKind.Power => Device.ModeParts.Length > 1 ? $"MODE {Device.Mode + 1}" : "ON / OFF",
        _ => "CHANGE MODE"
    };

    public string ChipTitle => Kind switch
    {
        DeviceActionKind.Part => PartChipName(Part),
        DeviceActionKind.Variant => CanSwitchVariant ? $"VAR {Device.VariantNumber}/{Device.VariantCount}" : "VAR",
        DeviceActionKind.Power => Device.ModeParts.Length > 1 ? $"POWER M{Device.Mode + 1}" : "POWER",
        _ => "MODE"
    };

    public string Footer => Kind switch
    {
        DeviceActionKind.Variant => CanSwitchVariant ? $"{Device.Name} · {Device.VariantNumber}/{Device.VariantCount}" : Device.Name,
        DeviceActionKind.NextMode => $"NEXT: MODE {(Device.Mode + 1) % Device.ModeParts.Length + 1}",
        _ => $"{Device.Name} · {(IsLit ? "ON" : "OFF")}"
    };

    public bool TryGetTarget(out LightsState state)
    {
        switch (Kind)
        {
            case DeviceActionKind.Part:
                state = Device.TogglePart(Part);
                return true;
            case DeviceActionKind.Variant:
                state = Device.NextVariant();
                return CanSwitchVariant;
            case DeviceActionKind.Power:
                state = Device.TogglePower();
                return true;
            default:
                state = Device.NextMode();
                return true;
        }
    }

    private bool CanSwitchVariant => Device.Light.IsActive && Device.VariantCount > 1;

    private static string PartName(DeviceParts part) => part switch
    {
        DeviceParts.VisLaser => "VIS LASER",
        DeviceParts.IrLaser => "IR LASER",
        DeviceParts.Light => "TAC LIGHT",
        _ => "IR ILLUMINATOR"
    };

    private static string PartChipName(DeviceParts part) => part switch
    {
        DeviceParts.VisLaser => "VIS",
        DeviceParts.IrLaser => "IR",
        DeviceParts.Light => "LIGHT",
        _ => "ILLUM"
    };
}
