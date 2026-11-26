namespace BetterTacticalDevicesHandling.Client.Devices;

internal static class Constants
{
    internal static class DeviceAction
    {
        internal const string NameVisLaser = "VIS LASER";
        internal const string NameIrLaser = "IR LASER";
        internal const string NameLight = "TAC LIGHT";
        internal const string NameIrIlluminator = "IR ILLUMINATOR";

        internal const string TitleVariant = "VARIANT";
        internal const string TitleModeNumber = "MODE {0}";
        internal const string TitleOnOff = "ON / OFF";
        internal const string TitleChangeMode = "CHANGE MODE";

        internal const string FooterNextMode = "NEXT: MODE {0}";
        internal const string FooterDeviceState = "{0} · {1}";
        internal const string FooterDeviceVariant = "{0} · {1}/{2}";

        internal const string ChipVisLaser = "VIS";
        internal const string ChipIrLaser = "IR";
        internal const string ChipLight = "LIGHT";
        internal const string ChipIrIlluminator = "ILLUM";
        internal const string ChipVariant = "VAR";
        internal const string ChipVariantNumber = "VAR {0}/{1}";
        internal const string ChipMode = "MODE";
        internal const string ChipPower = "POWER";
        internal const string ChipPowerMode = "POWER M{0}";

        internal const string LabelOn = "ON";
        internal const string LabelOff = "OFF";
    }

    internal static class DeviceModes
    {
        // Game's own (misspelt) field name.
        internal const string FieldModeTransforms = "_ligthbeamsTransforms";
        internal const string MatchIndicator = "indicator";
    }
}
