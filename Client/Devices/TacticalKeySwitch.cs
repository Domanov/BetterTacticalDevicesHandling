using EFT;

namespace BetterTacticalDevicesHandling.Client.Devices;

// T as a master switch: anything on → remember the setup and turn everything off; all off → bring the
// remembered setup back. Hold mode only restores (its key-up still turns everything off the vanilla way).
// UI Fixes' momentary hold runs in toggle mode and sends T on press and release, which maps onto restore/off.
internal static class TacticalKeySwitch
{
    // Returns false to leave the press to the game (no devices, or nothing remembered while all are off).
    public static bool Toggle(FirearmHandsInputTranslator translator)
    {
        LightsState[] states = translator.GetCurrentLightStates();
        if (states == null || states.Length == 0)
        {
            return false;
        }

        string weaponId = translator._controller.Item.Id;
        bool toggleMode = translator.IsPressTacticalInteraction;
        if (AnyOn(states))
        {
            if (!toggleMode)
            {
                return false;
            }

            LightSetups.Remember(weaponId, states);
            for (int i = 0; i < states.Length; i++)
            {
                states[i].IsActive = false;
            }
        }
        else if (!LightSetups.TryRestore(weaponId, states))
        {
            return false;
        }

        // Same call and sound rule as the vanilla toggle.
        if (translator._controller.SetLightsState(states, false, toggleMode) && !toggleMode)
        {
            translator._player.PlayTacticalSound();
        }

        return true;
    }

    private static bool AnyOn(LightsState[] states)
    {
        foreach (LightsState state in states)
        {
            if (state.IsActive)
            {
                return true;
            }
        }

        return false;
    }
}
