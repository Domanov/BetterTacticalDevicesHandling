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
    private const string NameVisLaser = "VIS LASER";
    private const string NameIrLaser = "IR LASER";
    private const string NameLight = "TAC LIGHT";
    private const string NameIrIlluminator = "IR ILLUMINATOR";

    private const string TitleVariant = "VARIANT";
    private const string TitleMode = "MODE";
    private const string TitleOnOff = "ON / OFF";
    private const string TitleChangeMode = "CHANGE MODE";

    private const string FooterNextMode = "NEXT: MODE";

    private const string ChipVisLaser = "VIS";
    private const string ChipIrLaser = "IR";
    private const string ChipLight = "LIGHT";
    private const string ChipIrIlluminator = "ILLUM";
    private const string ChipVariant = "VAR";
    private const string ChipMode = "MODE";
    private const string ChipPower = "POWER";
    private const string ChipModePrefix = "M";

    private const string LabelOn = "ON";
    private const string LabelOff = "OFF";
    private const string LabelSeparator = " · ";

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
        DeviceActionKind.Power => Device.ModeParts.Length > 1 ? $"{TitleMode} {Device.Mode + 1}" : TitleOnOff,
        _ => TitleChangeMode
    };

    public string ChipTitle => Kind switch
    {
        DeviceActionKind.Part => PartChipName(Part),
        DeviceActionKind.Variant => CanSwitchVariant ? $"{ChipVariant} {VariantFraction}" : ChipVariant,
        DeviceActionKind.Power => Device.ModeParts.Length > 1 ? $"{ChipPower} {ChipModePrefix}{Device.Mode + 1}" : ChipPower,
        _ => ChipMode
    };

    public string Footer => Kind switch
    {
        DeviceActionKind.Variant => CanSwitchVariant ? Device.Name + LabelSeparator + VariantFraction : Device.Name,
        DeviceActionKind.NextMode => $"{FooterNextMode} {(Device.Mode + 1) % Device.ModeParts.Length + 1}",
        _ => Device.Name + LabelSeparator + (IsLit ? LabelOn : LabelOff)
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
