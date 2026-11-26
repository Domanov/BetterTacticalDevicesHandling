using BetterTacticalDevicesHandling.Client.Devices;
using EFT;

namespace BetterTacticalDevicesHandling.Tests;

public class LightSetupsTests
{
    private static LightsState State(string id, bool on, int mode = 0) => new LightsState { Id = id, IsActive = on, LightMode = mode };

    [Fact]
    public void RestoresTheRememberedSetupByDeviceId()
    {
        LightSetups.Remember("weapon-a", new[] { State("light", false), State("lam", true, 2) });
        LightsState[] current = { State("lam", false), State("light", false) };

        Assert.True(LightSetups.TryRestore("weapon-a", current));
        Assert.Equal(State("lam", true, 2), current[0]);
        Assert.Equal(State("light", false), current[1]);
    }

    [Fact]
    public void AnAllOffSetupIsNotRemembered()
    {
        LightSetups.Remember("weapon-b", new[] { State("light", true) });
        LightSetups.Remember("weapon-b", new[] { State("light", false) });
        LightsState[] current = { State("light", false) };

        Assert.True(LightSetups.TryRestore("weapon-b", current));
        Assert.True(current[0].IsActive);
    }

    [Fact]
    public void NothingIsRestoredForAnUnknownWeapon() =>
        Assert.False(LightSetups.TryRestore("weapon-c", new[] { State("light", false) }));

    [Fact]
    public void NothingIsRestoredWhenNoRememberedDeviceIsStillOnTheWeapon()
    {
        LightSetups.Remember("weapon-d", new[] { State("old-light", true) });

        Assert.False(LightSetups.TryRestore("weapon-d", new[] { State("new-light", false) }));
    }
}
