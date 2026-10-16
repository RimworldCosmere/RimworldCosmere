using System;
using System.IO;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The dock offered a burn the pawn could not pay for, and starting it worked.
/// </summary>
[TestClass]
public class BurnAffordabilityTests {
    [TestMethod]
    public void AStepUpNeedsMetalAndAStepDownNeverDoes() {
        // off -> burning, with nothing in the reserve
        Assert.IsFalse(BurnAffordability.Allowed(0, 1, false));
        Assert.IsTrue(BurnAffordability.Allowed(0, 1, true));

        // burning -> flaring costs more, so it is still a step up
        Assert.IsFalse(BurnAffordability.Allowed(1, 2, false));
        Assert.IsTrue(BurnAffordability.Allowed(1, 2, true));
    }

    [TestMethod]
    public void EasingOffIsAlwaysAllowed() {
        // an empty reserve is exactly when a running burn most needs to stop
        Assert.IsTrue(BurnAffordability.Allowed(1, 0, false));
        Assert.IsTrue(BurnAffordability.Allowed(2, 1, false));
        Assert.IsTrue(BurnAffordability.Allowed(1, 1, false));
    }

    [TestMethod]
    public void TheDockPricesABurnBeforeStartingIt() {
        string source = Section();
        Assert.Contains("BurnAffordability.Allowed", source);

        // the old bypass: a new status applied with nothing asking what it cost
        Assert.DoesNotContain("a.UpdateStatus(BurnToggle.Next(a.status, flare));", source);
    }

    [TestMethod]
    public void TheWheelPricesABurnTheSameWayTheDockDoes() {
        string source = Ability();

        // one rule for both: CanBurn, charged at the status the cast would move to
        Assert.Contains("Gene.CanBurn(GetDesiredBurnRateForStatus(", source);
        Assert.DoesNotContain("return base.CanCast;", source);
    }

    private static string Ability() {
        return Read("System", "Scadrial", "Allomancy", "Ability", "AllomancyAbility.cs");
    }

    private static string Section() {
        return Read("System", "Scadrial", "UI", "AllomancyDockSection.cs");
    }

    private static string Read(params string[] parts) {
        DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereCore", "CosmereCore"))) {
            dir = dir.Parent;
        }

        Assert.IsNotNull(dir);

        return File.ReadAllText(Path.Combine(
            [dir.FullName, "CosmereCore", "CosmereCore", .. parts]
        ));
    }
}
