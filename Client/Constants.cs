namespace BetterTacticalDevicesHandling.Client;

internal static class Constants
{
    internal static class Plugin
    {
        internal const string GuidSptCore = "com.SPT.core";

        internal const string ConfigSectionWheel = "Wheel";
        internal const string ConfigKeyWheelKey = "Wheel key";
        internal const string ConfigDescriptionWheelKey =
            "Opens the device wheel while a weapon is in your hands; press again or Esc to close. Clear it to turn the wheel off. "
            + "Middle mouse is also the game's free look key by default.";
        internal const string ConfigKeyActivation = "Activation";
        internal const string ConfigDescriptionActivation =
            "Toggle: press the wheel key to open, click tiles, press it again (or Esc / right mouse) to close. "
            + "Hold: hold the wheel key, point at a tile and release the key to apply it.";
        internal const string ConfigKeyMoveWhileOpen = "Move while open";
        internal const string ConfigDescriptionMoveWhileOpen =
            "Keep walking, sprinting, crouching, jumping and leaning while the wheel is open. Mouse look stays blocked, so the mouse picks tiles.";

        internal const string ConfigSectionInspect = "Inspect window";
        internal const string ConfigKeyInspectPanel = "Show device panel";
        internal const string ConfigDescriptionInspectPanel = "Show the device switches in the 3D preview of a weapon's inspect window.";

        internal const string ConfigSectionDebug = "Debug";
        internal const string ConfigKeyLogModes = "Log device modes";
        internal const string ConfigDescriptionLogModes =
            "Log the parts found in each device mode the first time a device type shows up in the wheel (to check modded devices).";
    }

    internal static class UiFont
    {
        internal const string MatchGameFont = "Bender";
    }
}
