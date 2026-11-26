# BetterTacticalDevicesHandling

Author: Dmnv · GUID: `com.dmnv.bettertacticaldeviceshandling` · Target: SPT 4.1.x client (BepInEx plugin) + server mod · Fika-safe

Press middle mouse (configurable in F12) to open a wheel with every light and laser on the weapon in your hands. Switch each part (visible/IR laser, light, IR illuminator) on its own, within the combinations the device offers; press it again, Esc or right mouse to close. In F12 you can switch the wheel to hold mode (hold the key, point at a tile, release to apply), and by default you can keep moving while it's open.

The tactical device key (`T`) switches between all off and the last setup you made in the wheel or inspect panel, per weapon (F12 option, on by default; Ctrl+T is unchanged). Devices the game forgets to refresh visually when several are mounted are refreshed too.

The inspect window of a weapon also gets a small panel with the same per-part switches. Out of raid the change is saved to your profile by the server part; in raid it works for the weapon in your hands.

Install: client `BepInEx\plugins\Dmnv-BetterTacticalDevicesHandling\` (`BetterTacticalDevicesHandling.Client.dll`), server `SPT_Runtime\user\mods\Dmnv-BetterTacticalDevicesHandling\` (needed to save out-of-raid changes; in Fika it goes on the server, the client part is optional per player).

Build: open `BetterTacticalDevicesHandling.slnx` (VS 2026 / Rider) or `dotnet build`. Release zip: `dotnet build -c Release` → `dist\`.
