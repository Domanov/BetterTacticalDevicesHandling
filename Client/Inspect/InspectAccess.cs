using Comfort.Common;
using EFT;
using EFT.InventoryLogic;

namespace BetterTacticalDevicesHandling.Client.Inspect;

internal enum InspectAccess
{
    Hidden,
    // In raid, own weapon not in hands: only the hands controller syncs light changes to Fika, so it stays read-only.
    Locked,
    InHands,
    // Out of raid or in the hideout: changed locally and saved through the server route.
    Stash
}

internal static class InspectAccessRules
{
    public static InspectAccess For(Weapon weapon)
    {
        if (!Singleton<GameWorld>.Instantiated || Singleton<GameWorld>.Instance is HideoutGameWorld)
        {
            return weapon.Owner is InventoryController ? InspectAccess.Stash : InspectAccess.Hidden;
        }

        Player player = Singleton<GameWorld>.Instance.MainPlayer;
        if (player == null)
        {
            return InspectAccess.Hidden;
        }

        if (player.HandsController is Player.FirearmController firearm && firearm.Item == weapon)
        {
            return InspectAccess.InHands;
        }

        return ReferenceEquals(weapon.Owner, player.InventoryController) ? InspectAccess.Locked : InspectAccess.Hidden;
    }
}
