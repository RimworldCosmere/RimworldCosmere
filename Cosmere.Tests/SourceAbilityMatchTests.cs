using System.Collections.Generic;
using Cosmere.Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     A saved hediff has to come back held by the same ability that was holding it. These pin the
///     lookup that puts it back.
/// </summary>
[TestClass]
public class SourceAbilityMatchTests {
    private static readonly List<string> Mistborn = [
        "Cosmere_Scadrial_Ability_SteelPush",
        "Cosmere_Scadrial_Ability_CopperAura",
        "Cosmere_Scadrial_Ability_IronPull",
    ];

    [TestMethod]
    public void TheWantedAbilityIsFound() {
        Assert.AreEqual(1, SourceAbilityMatch.IndexOf(Mistborn, "Cosmere_Scadrial_Ability_CopperAura"));
    }

    [TestMethod]
    public void ItIsNotJustTheFirstOneInTheRoster() {
        // bug this guards: a coppercloud reloaded as whatever ability sat first on its Smoker, arbitrary on a Mistborn.
        Assert.AreNotEqual(0, SourceAbilityMatch.IndexOf(Mistborn, "Cosmere_Scadrial_Ability_CopperAura"));
        Assert.AreEqual(2, SourceAbilityMatch.IndexOf(Mistborn, "Cosmere_Scadrial_Ability_IronPull"));
    }

    [TestMethod]
    public void AnAbilityThePawnNoLongerHasIsMissing() {
        // Spiked out between save and load, or the mod that added it removed. Not an error.
        Assert.AreEqual(-1, SourceAbilityMatch.IndexOf(Mistborn, "Cosmere_Scadrial_Ability_BronzeAura"));
    }

    [TestMethod]
    public void AnEmptyRosterMatchesNothing() {
        Assert.AreEqual(-1, SourceAbilityMatch.IndexOf([], "Cosmere_Scadrial_Ability_CopperAura"));
    }

    [TestMethod]
    public void NothingWantedMatchesNothing() {
        Assert.AreEqual(-1, SourceAbilityMatch.IndexOf(Mistborn, null));
        Assert.AreEqual(-1, SourceAbilityMatch.IndexOf(Mistborn, string.Empty));
    }

    [TestMethod]
    public void TheFirstOfARepeatedDefWins() {
        List<string> doubled = ["A", "B", "B"];

        Assert.AreEqual(1, SourceAbilityMatch.IndexOf(doubled, "B"));
    }
}
