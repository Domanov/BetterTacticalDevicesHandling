using BetterTacticalDevicesHandling.Client.Devices;

namespace BetterTacticalDevicesHandling.Tests;

public class ModePickerTests
{
    private const DeviceParts V = DeviceParts.VisLaser;
    private const DeviceParts R = DeviceParts.IrLaser;
    private const DeviceParts L = DeviceParts.Light;
    private const DeviceParts I = DeviceParts.IrIlluminator;

    private static readonly DeviceParts[] Peq15 = { V, R, I, I | R };
    private static readonly DeviceParts[] DbalA4 = { V, V, L | V, L | V, L, I | R, I | R };
    private static readonly DeviceParts[] Ctf3 = { L, R, I | R, I };
    private static readonly DeviceParts[] Raptar = { DeviceParts.None, V, R, I, I | R };

    [Fact]
    public void TurningOnAPartFromOffPicksItsMode() => Assert.Equal(0, ModePicker.TogglePart(Peq15, false, 3, V));

    [Fact]
    public void AddingAPartPicksTheCombinedMode() => Assert.Equal(3, ModePicker.TogglePart(Peq15, true, 2, R));

    [Fact]
    public void RemovingAPartKeepsTheRest() => Assert.Equal(1, ModePicker.TogglePart(Peq15, true, 3, I));

    [Fact]
    public void RemovingTheLastPartTurnsTheDeviceOff() => Assert.Equal(ModePicker.Off, ModePicker.TogglePart(Peq15, true, 0, V));

    [Fact]
    public void AMissingCombinationLetsTheToggledPartWin() => Assert.Equal(0, ModePicker.TogglePart(Peq15, true, 3, V));

    [Fact]
    public void AMissingSinglePartFallsBackToTheClosestModeWithIt() => Assert.Equal(5, ModePicker.TogglePart(DbalA4, false, 0, R));

    [Fact]
    public void SwitchingPartsKeepsThePowerLevel() => Assert.Equal(3, ModePicker.TogglePart(DbalA4, true, 1, L));

    [Fact]
    public void MixedSpectraPreferTheModeWithoutExtras() => Assert.Equal(1, ModePicker.TogglePart(Ctf3, true, 0, R));

    [Fact]
    public void ModesWithoutPartsAreNeverPicked() => Assert.Equal(1, ModePicker.TogglePart(Raptar, false, 0, V));

    [Fact]
    public void NextVariantCyclesModesWithTheSameParts()
    {
        Assert.Equal(1, ModePicker.NextVariant(DbalA4, 0));
        Assert.Equal(0, ModePicker.NextVariant(DbalA4, 1));
        Assert.Equal(4, ModePicker.NextVariant(DbalA4, 4));
    }

    [Fact]
    public void VariantCountAndIndexFollowTheCurrentParts()
    {
        Assert.Equal(2, ModePicker.VariantCount(DbalA4, 3));
        Assert.Equal(1, ModePicker.VariantIndex(DbalA4, 3));
        Assert.Equal(1, ModePicker.VariantCount(DbalA4, 4));
    }
}
