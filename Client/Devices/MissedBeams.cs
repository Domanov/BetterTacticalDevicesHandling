using System;
using System.Collections.Generic;
using EFT;

namespace BetterTacticalDevicesHandling.Client.Devices;

// Firearms.UpdateBeams only reaches _tacticalComboVisualControllers, which the game builds with
// GetComponentsInChildrenActiveIgnoreFirstLevel: a device with an inactive parent at that moment is left out.
// It still changes state (T, Ctrl+T, the wheel, Fika) but its beams never update, so it is refreshed here.
internal static class MissedBeams
{
    public static void Refresh(Firearms firearms)
    {
        bool isYourPlayer = firearms.Player != null && firearms.Player.IsYourPlayer;
        foreach (TacticalComboVisualController visual in Missing(firearms))
        {
            // Init (in Awake) only runs once the object is active; UpdateBeams before that throws.
            if (visual.gameObject.activeInHierarchy)
            {
                visual.UpdateBeams(isYourPlayer);
            }
        }
    }

    public static List<TacticalComboVisualController> Missing(Firearms firearms)
    {
        List<TacticalComboVisualController> missing = new List<TacticalComboVisualController>();
        if (firearms._weaponHierarchy == null)
        {
            return missing;
        }

        TacticalComboVisualController[] listed = firearms._tacticalComboVisualControllers;
        foreach (TacticalComboVisualController visual in firearms._weaponHierarchy.GetComponentsInChildren<TacticalComboVisualController>(true))
        {
            if (visual.LightMod != null && (listed == null || Array.IndexOf(listed, visual) < 0))
            {
                missing.Add(visual);
            }
        }

        return missing;
    }
}
