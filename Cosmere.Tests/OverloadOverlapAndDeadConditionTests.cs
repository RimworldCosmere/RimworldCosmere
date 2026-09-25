using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>Shadowed constructor defaults and conditions that could only land one way.</summary>
/// <remarks>Every site is Verse-bound: no Scribe and no Widgets here, so these read the source.</remarks>
[TestClass]
public partial class OverloadOverlapAndDeadConditionTests {
    private static string CoreRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate CosmereScadrial/Defs above the test output directory.");
            return Path.Combine(dir!.FullName, "CosmereCore", "CosmereCore");
        }
    }

    /// <summary>
    ///     The two-argument constructor took every call SpiritWeb could have made, so the bond
    ///     flags on the four-argument one were unreachable defaults rather than a choice.
    /// </summary>
    [TestMethod]
    public void ConnectionDeclaresOneConstructorForTheTwoBondedObjects() {
        string source = SourceOf(Path.Combine("Core", "Comp", "Game", "Connection.cs"));

        Assert.IsFalse(
            TwoArgumentConnectionConstructor.IsMatch(source),
            "Connection declares a two-argument constructor that shadows the defaults on the " +
            "four-argument one. Keep only the constructor that takes the bond flags."
        );
        Assert.IsTrue(
            BondFlagDefault.IsMatch(source),
            "Connection must still offer the bond flags with their defaults."
        );
    }

    /// <summary>
    ///     InnerStorage hands Scribe_Deep three constructor arguments and Verse rebuilds the
    ///     container with Activator, so only an exactly-three-parameter constructor answers.
    /// </summary>
    [TestMethod]
    public void ThingOwnerWithCapacityKeepsTheArityScribeRebuildsItWith() {
        string source = SourceOf(Path.Combine("Core", "Comp", "Thing", "ThingOwnerWithCapacity.cs"));

        Assert.IsTrue(
            ScribeRebuildableConstructor.IsMatch(source),
            "ThingOwnerWithCapacity lost the (IThingHolder, int, bool) constructor. Scribe's " +
            "Activator call cannot bind optional parameters, so loading a saved container breaks."
        );
        Assert.IsFalse(
            OptionalTailAfterOneStackOnly.IsMatch(source),
            "A constructor adds optional parameters after (owner, maxCapacity, oneStackOnly). " +
            "Those defaults can never be reached, because the shorter overload wins every call."
        );
    }

    /// <summary>
    ///     The reason the shorter constructor has to stay: ScribeExtractor.CreateInstance passes
    ///     Instance|Public|NonPublic and no OptionalParamBinding.
    /// </summary>
    [TestMethod]
    public void ActivatorDoesNotFillInOptionalConstructorParameters() {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        Assert.ThrowsExactly<MissingMethodException>(
            () => Activator.CreateInstance(typeof(OnlyOptionalTail), flags, null, [7], null, null),
            "Activator matched a constructor by filling in a default. If the runtime really does " +
            "that now, ThingOwnerWithCapacity no longer needs its three-parameter constructor."
        );
    }

    /// <summary>
    ///     A Thing target reads back Invalid during LoadingVars and only becomes the real Thing in
    ///     ResolvingCrossRefs, so a LoadingVars-only read loses every saved target.
    /// </summary>
    [TestMethod]
    public void AbilityTargetIsScribedInEveryModeNotOnlyLoadingVars() {
        string source = SourceOf(Path.Combine("Core", "Ability", "AbstractAbility.cs"));

        Assert.IsFalse(
            source.Contains("targetPresent", StringComparison.Ordinal),
            "ExposeData still gates the target on a targetPresent flag read in LoadingVars. " +
            "A Thing target is not resolved yet in that mode, so it loads back as Invalid."
        );
        Assert.IsTrue(
            UnconditionalTargetLook.IsMatch(source),
            "ExposeData must run Scribe_TargetInfo.Look unconditionally so ResolvingCrossRefs reaches it."
        );
        Assert.IsTrue(
            ResolvedTargetWriteBack.IsMatch(source),
            "ExposeData must write the resolved target back in every load mode, not just LoadingVars."
        );
    }

    /// <summary>
    ///     FillTab returns early when no system is invested, so the switcher test below it could
    ///     only ever be true and the zero-provider layout it guarded was unreachable.
    /// </summary>
    [TestMethod]
    public void CodexTabDoesNotRetestASwitcherItAlreadyReturnedWithout() {
        string source = SourceOf(Path.Combine("Core", "Tab", "ITab_Investiture.cs"));

        Assert.IsTrue(
            EmptyProviderEarlyReturn.IsMatch(source),
            "FillTab must keep the early return for a pawn with no invested system."
        );
        Assert.IsFalse(
            ProviderCountRetest.IsMatch(source),
            "FillTab tests investedProviders.Count > 0 after already returning on Count == 0. " +
            "That test is always true and the layout behind its false branch is dead."
        );
    }

    /// <summary>
    ///     Widgets.CheckboxLabeled skips its toggle entirely when disabled, and a required Shard
    ///     passes disabled, so the row could never see a changed value to re-reject.
    /// </summary>
    [TestMethod]
    public void RequiredShardRowDoesNotRecheckWhatTheCheckboxAlreadyDisabled() {
        string source = SourceOf(Path.Combine("Core", "UI", "Dialog_SelectShards.cs"));

        Assert.IsFalse(
            RequiredShardRetest.IsMatch(source),
            "DrawShardRow re-tests isRequired after passing it to CheckboxLabeled as disabled. " +
            "A disabled checkbox never writes checkOn, so that half of the condition is dead."
        );
        Assert.IsTrue(
            ChangedCheckboxGuard.IsMatch(source),
            "DrawShardRow must still only touch settings when the player actually changed the box."
        );
    }

    /// <summary>
    ///     Describe returned null twice for one case: on an empty def list, and on a label list
    ///     that could only be empty when that def list was.
    /// </summary>
    [TestMethod]
    public void RemovedFactionDescriptionChecksTheJoinedNamesOnce() {
        string source = SourceOf(Path.Combine("Core", "ScenarioPart", "Action", "RemoveFactionAction.cs"));

        Assert.IsFalse(
            EmptyFactionListEarlyReturn.IsMatch(source),
            "Describe keeps an early return that the empty-names check below it already covers."
        );
        Assert.IsTrue(
            EmptyJoinedNamesCheck.IsMatch(source),
            "Describe must decide on the joined names, so a def with no label reads as nothing to name."
        );
    }

    private static string SourceOf(string relative) {
        string path = Path.Combine(CoreRoot, relative);
        Assert.IsTrue(File.Exists(path), $"Expected a source file at {path}");
        return File.ReadAllText(path).Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    [GeneratedRegex(
        @"public Connection\(\s*ILoadReferenceable\s+objectOne\s*,\s*ILoadReferenceable\s+objectTwo\s*\)"
    )]
    private static partial Regex TwoArgumentConnectionConstructor { get; }

    [GeneratedRegex(@"bool\s+canBondObjectOne\s*=\s*true")]
    private static partial Regex BondFlagDefault { get; }

    [GeneratedRegex(
        @"ThingOwnerWithCapacity\(\s*IThingHolder\s+owner\s*,\s*int\s+maxCapacity\s*,\s*bool\s+oneStackOnly\s*\)"
    )]
    private static partial Regex ScribeRebuildableConstructor { get; }

    [GeneratedRegex(@"int\s+maxCapacity\s*,\s*bool\s+oneStackOnly\s*,\s*LookMode\s+\w+\s*=")]
    private static partial Regex OptionalTailAfterOneStackOnly { get; }

    [GeneratedRegex(@"Scribe_TargetInfo\.Look\(\s*ref\s+target\s*,\s*""target""\s*\)")]
    private static partial Regex UnconditionalTargetLook { get; }

    [GeneratedRegex(@"Scribe\.mode\s*!=\s*LoadSaveMode\.Saving\s*\)\s*\{?\s*localTarget\s*=\s*target\.IsValid")]
    private static partial Regex ResolvedTargetWriteBack { get; }

    [GeneratedRegex(@"investedProviders\.Count\s*==\s*0")]
    private static partial Regex EmptyProviderEarlyReturn { get; }

    [GeneratedRegex(@"investedProviders\.Count\s*>\s*0")]
    private static partial Regex ProviderCountRetest { get; }

    [GeneratedRegex(@"toggled\s*!=\s*isEnabled\s*&&\s*!\s*isRequired")]
    private static partial Regex RequiredShardRetest { get; }

    [GeneratedRegex(@"if\s*\(\s*toggled\s*!=\s*isEnabled\s*\)")]
    private static partial Regex ChangedCheckboxGuard { get; }

    [GeneratedRegex(@"factions\.Count\s*==\s*0")]
    private static partial Regex EmptyFactionListEarlyReturn { get; }

    [GeneratedRegex(@"named\.Length\s*==\s*0")]
    private static partial Regex EmptyJoinedNamesCheck { get; }

    private sealed class OnlyOptionalTail {
        public OnlyOptionalTail(int required, bool optional = true) {
            Required = required;
            Optional = optional;
        }

        public int Required { get; }

        public bool Optional { get; }
    }
}
