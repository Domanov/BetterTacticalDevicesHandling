using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BetterTacticalDevicesHandling.Client.Inspect;
using BetterTacticalDevicesHandling.Client.Patches;
using BetterTacticalDevicesHandling.Client.Wheel;
using UnityEngine;
using static BetterTacticalDevicesHandling.Client.Constants.Plugin;

namespace BetterTacticalDevicesHandling.Client;

[BepInPlugin(Guid, Name, Version)]
[BepInDependency(GuidSptCore, BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BaseUnityPlugin
{
    public const string Guid = "com.dmnv.bettertacticaldeviceshandling";
    public const string Name = "Dmnv-BetterTacticalDevicesHandling";
    public const string Version = "1.0.0";

    internal static ManualLogSource Log;
    internal static ConfigEntry<KeyboardShortcut> WheelKey;
    internal static ConfigEntry<WheelActivation> Activation;
    internal static ConfigEntry<bool> MoveWhileOpen;
    internal static ConfigEntry<bool> InspectPanelEnabled;
    internal static ConfigEntry<bool> LogModes;

    private KeyCode _wheelKey;

    private void Awake()
    {
        Log = Logger;
        WheelKey = Config.Bind(ConfigSectionWheel, ConfigKeyWheelKey, new KeyboardShortcut(KeyCode.Mouse2), ConfigDescriptionWheelKey);
        WheelKey.SettingChanged += (_, _) => CacheWheelKey();
        CacheWheelKey();
        Activation = Config.Bind(ConfigSectionWheel, ConfigKeyActivation, WheelActivation.Toggle, ConfigDescriptionActivation);
        MoveWhileOpen = Config.Bind(ConfigSectionWheel, ConfigKeyMoveWhileOpen, true, ConfigDescriptionMoveWhileOpen);
        InspectPanelEnabled = Config.Bind(ConfigSectionInspect, ConfigKeyInspectPanel, true, ConfigDescriptionInspectPanel);
        InspectPanelEnabled.SettingChanged += (_, _) => InspectPanel.RefreshAll();
        LogModes = Config.Bind(ConfigSectionDebug, ConfigKeyLogModes, false, ConfigDescriptionLogModes);
        new InspectShowPatch().Enable();
        new InspectClosePatch().Enable();
        new PreviewBuiltPatch().Enable();
        new FirearmsBeamsPatch().Enable();
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
        else if (Input.GetKeyUp(_wheelKey) && Activation.Value == WheelActivation.Hold)
        {
            DeviceWheel.ApplyAndClose();
        }
    }
}
