using System.Collections.Generic;
using EFT;

namespace BetterTacticalDevicesHandling.Client.Devices;

// The last setup with something on, per weapon item id, for the T master switch. Session memory only.
internal static class LightSetups
{
    private static readonly Dictionary<string, LightsState[]> Setups = new Dictionary<string, LightsState[]>();

    public static void Remember(string weaponId, LightsState[] states)
    {
        foreach (LightsState state in states)
        {
            if (state.IsActive)
            {
                Setups[weaponId] = (LightsState[])states.Clone();
                return;
            }
        }
    }

    // Copies the remembered state onto matching devices; false when nothing remembered would be on.
    public static bool TryRestore(string weaponId, LightsState[] states)
    {
        if (!Setups.TryGetValue(weaponId, out LightsState[] saved))
        {
            return false;
        }

        bool anyOn = false;
        for (int i = 0; i < states.Length; i++)
        {
            foreach (LightsState state in saved)
            {
                if (state.Id == states[i].Id)
                {
                    states[i] = state;
                    anyOn |= state.IsActive;
                }
            }
        }

        return anyOn;
    }
}
