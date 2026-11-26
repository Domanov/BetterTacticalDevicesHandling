using System.Collections.Generic;
using EFT;
using static BetterTacticalDevicesHandling.Client.Devices.Constants.DeviceAction;

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
        DeviceActionKind.Variant => TitleVariant,
        DeviceActionKind.Power => Device.ModeParts.Length > 1 ? string.Format(TitleModeNumber, Device.Mode + 1) : TitleOnOff,
        _ => TitleChangeMode
    };

    public string ChipTitle => Kind switch
    {
        DeviceActionKind.Part => PartChipName(Part),
        DeviceActionKind.Variant => CanSwitchVariant
            ? string.Format(ChipVariantNumber, Device.VariantNumber, Device.VariantCount)
            : ChipVariant,
        DeviceActionKind.Power => Device.ModeParts.Length > 1 ? string.Format(ChipPowerMode, Device.Mode + 1) : ChipPower,
        _ => ChipMode
    };

    public string Footer => Kind switch
    {
        DeviceActionKind.Variant => CanSwitchVariant
            ? string.Format(FooterDeviceVariant, Device.Name, Device.VariantNumber, Device.VariantCount)
            : Device.Name,
        DeviceActionKind.NextMode => string.Format(FooterNextMode, (Device.Mode + 1) % Device.ModeParts.Length + 1),
        _ => string.Format(FooterDeviceState, Device.Name, IsLit ? LabelOn : LabelOff)
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
        DeviceParts.VisLaser => NameVisLaser,
        DeviceParts.IrLaser => NameIrLaser,
        DeviceParts.Light => NameLight,
        _ => NameIrIlluminator
    };

    private static string PartChipName(DeviceParts part) => part switch
    {
        DeviceParts.VisLaser => ChipVisLaser,
        DeviceParts.IrLaser => ChipIrLaser,
        DeviceParts.Light => ChipLight,
        _ => ChipIrIlluminator
    };
}
