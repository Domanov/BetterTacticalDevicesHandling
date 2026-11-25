using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BetterTacticalDevicesHandling.Client.Inspect;
using BetterTacticalDevicesHandling.Client.Patches;
using BetterTacticalDevicesHandling.Client.Wheel;
using UnityEngine;

namespace BetterTacticalDevicesHandling.Client;

[BepInPlugin(Guid, Name, Version)]
[BepInDependency(SptCoreGuid, BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BaseUnityPlugin
{
    public const string Guid = "com.dmnv.bettertacticaldeviceshandling";
    public const string Name = "Dmnv-BetterTacticalDevicesHandling";
    public const string Version = "1.0.0";

    private const string SptCoreGuid = "com.SPT.core";

    private const string WheelSection = "Wheel";
    private const string WheelKeyName = "Wheel key";
    private const string WheelKeyDescription =
        "Opens the device wheel while a weapon is in your hands; press again or Esc to close. Clear it to turn the wheel off. "
        + "Middle mouse is also the game's free look key by default.";

    private const string InspectSection = "Inspect window";
    private const string InspectPanelName = "Show device panel";
    private const string InspectPanelDescription = "Show the device switches in the 3D preview of a weapon's inspect window.";

    private const string DebugSection = "Debug";
    private const string LogModesName = "Log device modes";
    private const string LogModesDescription =
        "Log the parts found in each device mode the first time a device type shows up in the wheel (to check modded devices).";

    internal static ManualLogSource Log;
    internal static ConfigEntry<KeyboardShortcut> WheelKey;
    internal static ConfigEntry<bool> InspectPanelEnabled;
    internal static ConfigEntry<bool> LogModes;

    private KeyCode _wheelKey;

    private void Awake()
    {
        Log = Logger;
        WheelKey = Config.Bind(WheelSection, WheelKeyName, new KeyboardShortcut(KeyCode.Mouse2), WheelKeyDescription);
        WheelKey.SettingChanged += (_, _) => CacheWheelKey();
        CacheWheelKey();
        InspectPanelEnabled = Config.Bind(InspectSection, InspectPanelName, true, InspectPanelDescription);
        InspectPanelEnabled.SettingChanged += (_, _) => InspectPanel.RefreshAll();
        LogModes = Config.Bind(DebugSection, LogModesName, false, LogModesDescription);
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
