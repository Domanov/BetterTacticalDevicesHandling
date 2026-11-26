namespace BetterTacticalDevicesHandling.Client.Inspect;

internal static class Constants
{
    internal static class InspectPanel
    {
        internal const string FieldCachedLightMasks = "_cachedLightMasks";
        internal const string FieldPositionedObject = "_positionedObject";

        internal const string ObjectPanel = Plugin.Name + " Panel";
        internal const string ObjectRow = "Row";
        internal const string ObjectChip = "Chip";
        internal const string ObjectText = "Text";

        internal const string ChipAllOff = "ALL OFF";

        internal const string LabelLockedHint = "Take the weapon in hands to switch";
    }

    internal static class LightStateStore
    {
        // Route and JSON keys must match LightStateApi in Server/LightStateRouter.cs.
        internal const string RouteLights = "/dmnv/bettertacticaldeviceshandling/lights";

        internal const string JsonItems = "items";
        internal const string JsonId = "id";
        internal const string JsonIsActive = "isActive";
        internal const string JsonMode = "mode";
        internal const string JsonFailed = "failed";
    }
}
