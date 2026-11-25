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
    private const string VariantTitle = "VARIANT";
    private const string ModeTitle = "MODE";
    private const string OnOffTitle = "ON / OFF";
    private const string ChangeModeTitle = "CHANGE MODE";
    private const string NextModeLabel = "NEXT: MODE";
    private const string VariantChip = "VAR";
    private const string PowerChip = "POWER";
    private const string ModeChipPrefix = "M";
    private const string OnLabel = "ON";
    private const string OffLabel = "OFF";
    private const string Separator = " · ";

    private const string VisLaserName = "VIS LASER";
    private const string IrLaserName = "IR LASER";
    private const string LightName = "TAC LIGHT";
    private const string IrIlluminatorName = "IR ILLUMINATOR";
    private const string VisLaserChip = "VIS";
    private const string IrLaserChip = "IR";
    private const string LightChip = "LIGHT";
    private const string IrIlluminatorChip = "ILLUM";

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
        DeviceActionKind.Variant => VariantTitle,
        DeviceActionKind.Power => Device.ModeParts.Length > 1 ? $"{ModeTitle} {Device.Mode + 1}" : OnOffTitle,
        _ => ChangeModeTitle
    };

    public string ChipTitle => Kind switch
    {
        DeviceActionKind.Part => PartChipName(Part),
        DeviceActionKind.Variant => CanSwitchVariant ? $"{VariantChip} {VariantFraction}" : VariantChip,
        DeviceActionKind.Power => Device.ModeParts.Length > 1 ? $"{PowerChip} {ModeChipPrefix}{Device.Mode + 1}" : PowerChip,
        _ => ModeTitle
    };

    public string Footer => Kind switch
    {
        DeviceActionKind.Variant => CanSwitchVariant ? Device.Name + Separator + VariantFraction : Device.Name,
        DeviceActionKind.NextMode => $"{NextModeLabel} {(Device.Mode + 1) % Device.ModeParts.Length + 1}",
        _ => Device.Name + Separator + (IsLit ? OnLabel : OffLabel)
    };

    private string VariantFraction => $"{Device.VariantNumber}/{Device.VariantCount}";

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
        DeviceParts.VisLaser => VisLaserName,
        DeviceParts.IrLaser => IrLaserName,
        DeviceParts.Light => LightName,
        _ => IrIlluminatorName
    };

    private static string PartChipName(DeviceParts part) => part switch
    {
        DeviceParts.VisLaser => VisLaserChip,
        DeviceParts.IrLaser => IrLaserChip,
        DeviceParts.Light => LightChip,
        _ => IrIlluminatorChip
    };
}
