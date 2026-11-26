using UnityEngine;

namespace BetterTacticalDevicesHandling.Client;

// Shared by the wheel and the inspect panel so an active device reads the same in both.
internal static class UiPalette
{
    public static readonly Color ActiveBackground = new Color(0.24f, 0.4f, 0.16f, 0.9f);
    public static readonly Color ActiveBackgroundSelected = new Color(0.33f, 0.53f, 0.22f, 0.95f);
    public static readonly Color TextActive = new Color(1f, 1f, 0.95f, 1f);
    public static readonly Color TextOn = new Color(0.92f, 0.92f, 0.88f, 1f);
    public static readonly Color TextOff = new Color(0.55f, 0.55f, 0.52f, 1f);
}
