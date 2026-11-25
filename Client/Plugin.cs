using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BetterTacticalDevicesHandling.Client.Inspect;
using BetterTacticalDevicesHandling.Client.Patches;
using BetterTacticalDevicesHandling.Client.Wheel;
using UnityEngine;

namespace BetterTacticalDevicesHandling.Client;

[BepInPlugin(Guid, Name, Version)]
[BepInDependency(GuidSptCore, BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BaseUnityPlugin
{
    public const string Guid = "com.dmnv.bettertacticaldeviceshandling";
    public const string Name = "Dmnv-BetterTacticalDevicesHandling";
    public const string Version = "1.0.0";

    private const string GuidSptCore = "com.SPT.core";

    private const string ConfigSectionWheel = "Wheel";
    private const string ConfigKeyWheelKey = "Wheel key";
    private const string ConfigDescriptionWheelKey =
        "Opens the device wheel while a weapon is in your hands; press again or Esc to close. Clear it to turn the wheel off. "
        + "Middle mouse is also the game's free look key by default.";

    private const string ConfigSectionInspect = "Inspect window";
    private const string ConfigKeyInspectPanel = "Show device panel";
    private const string ConfigDescriptionInspectPanel = "Show the device switches in the 3D preview of a weapon's inspect window.";

    private const string ConfigSectionDebug = "Debug";
    private const string ConfigKeyLogModes = "Log device modes";
    private const string ConfigDescriptionLogModes =
        "Log the parts found in each device mode the first time a device type shows up in the wheel (to check modded devices).";

    internal static ManualLogSource Log;
    internal static ConfigEntry<KeyboardShortcut> WheelKey;
    internal static ConfigEntry<bool> InspectPanelEnabled;
    internal static ConfigEntry<bool> LogModes;

    private KeyCode _wheelKey;

    private void Awake()
    {
        Log = Logger;
        WheelKey = Config.Bind(ConfigSectionWheel, ConfigKeyWheelKey, new KeyboardShortcut(KeyCode.Mouse2), ConfigDescriptionWheelKey);
        WheelKey.SettingChanged += (_, _) => CacheWheelKey();
        CacheWheelKey();
        InspectPanelEnabled = Config.Bind(ConfigSectionInspect, ConfigKeyInspectPanel, true, ConfigDescriptionInspectPanel);
        InspectPanelEnabled.SettingChanged += (_, _) => InspectPanel.RefreshAll();
        LogModes = Config.Bind(ConfigSectionDebug, ConfigKeyLogModes, false, ConfigDescriptionLogModes);
        new InspectShowPatch().Enable();
        new InspectClosePatch().Enable();
        new PreviewBuiltPatch().Enable();
        Log.LogInfo($"{Name} {Version} loaded");
    }

    // Update is the only per-frame work: it stays off without a wheel key, and the full shortcut check
    // (modifier keys) only runs on the frame the main key goes down.
    private void CacheWheelKey()
    {
        _wheelKey = WheelKey.Value.MainKey;
        enabled = _wheelKey != KeyCode.None;
    }

    private void Update()
    {
        if (Input.GetKeyDown(_wheelKey) && WheelKey.Value.IsDown())
        {
            DeviceWheel.Toggle();
        }
    }
}
