using System;
using System.Collections.Generic;

namespace BetterTacticalDevicesHandling.Client.Devices;

[Flags]
internal enum DeviceParts
{
    None = 0,
    VisLaser = 1,
    IrLaser = 2,
    Light = 4,
    IrIlluminator = 8
}

// A device can only show the part combinations baked into its modes, so part toggles are mapped onto the closest mode.
internal static class ModePicker
{
    public const int Off = -1;

    public static int TogglePart(IReadOnlyList<DeviceParts> modes, bool active, int current, DeviceParts part)
    {
        DeviceParts have = active ? modes[current] : DeviceParts.None;
        bool turnOn = (have & part) == 0;
        DeviceParts want = turnOn ? have | part : have & ~part;
        if (want == DeviceParts.None)
        {
            return Off;
        }

        int exact = NthWithParts(modes, want, active ? VariantIndex(modes, current) : 0);
        if (exact != Off)
        {
            return exact;
        }

        int best = Off;
        int bestOverlap = -1;
        int bestExtras = int.MaxValue;
        for (int i = 0; i < modes.Count; i++)
        {
            DeviceParts parts = modes[i];
            if (parts == DeviceParts.None || ((parts & part) != 0) != turnOn)
            {
                continue;
            }

            int overlap = Count(parts & want);
            int extras = Count(parts & ~want);
            if (overlap > bestOverlap || (overlap == bestOverlap && extras < bestExtras))
            {
                best = i;
                bestOverlap = overlap;
                bestExtras = extras;
            }
        }

        return best;
    }

    public static int NextVariant(IReadOnlyList<DeviceParts> modes, int current)
    {
        for (int step = 1; step < modes.Count; step++)
        {
            int i = (current + step) % modes.Count;
            if (modes[i] == modes[current])
            {
                return i;
            }
        }

        return current;
    }

    public static int VariantCount(IReadOnlyList<DeviceParts> modes, int current)
    {
        int count = 0;
        foreach (DeviceParts parts in modes)
        {
            if (parts == modes[current])
            {
                count++;
            }
        }

        return count;
    }

    // Position of the current mode among the modes with the same parts (power levels), kept when switching parts.
    public static int VariantIndex(IReadOnlyList<DeviceParts> modes, int current)
    {
        int index = 0;
        for (int i = 0; i < current; i++)
        {
            if (modes[i] == modes[current])
            {
                index++;
            }
        }

        return index;
    }

    private static int NthWithParts(IReadOnlyList<DeviceParts> modes, DeviceParts parts, int n)
    {
        int last = Off;
        for (int i = 0; i < modes.Count; i++)
        {
            if (modes[i] != parts)
            {
                continue;
            }

            if (n-- == 0)
            {
                return i;
            }

            last = i;
        }

        return last;
    }

    private static int Count(DeviceParts parts)
    {
        int count = 0;
        for (int bits = (int)parts; bits != 0; bits &= bits - 1)
        {
            count++;
        }

        return count;
    }
}
