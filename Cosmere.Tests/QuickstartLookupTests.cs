using System;
using System.Collections.Generic;
using Cosmere.Core.Quickstart;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

[TestClass]
public class QuickstartLookupTests {
    private static readonly IReadOnlyList<Type> Candidates = [
        typeof(Scadrial.PreCatacendreQuickstart),
        typeof(Roshar.WindrunnerQuickstart),
        typeof(Roshar.PreCatacendreQuickstart),
    ];

    [TestMethod]
    public void ResolvesAnUnambiguousShortName() {
        Type? resolved = QuickstartLookup.Resolve("WindrunnerQuickstart", Candidates, out string? error);

        Assert.AreSame(typeof(Roshar.WindrunnerQuickstart), resolved);
        Assert.IsNull(error);
    }

    [TestMethod]
    public void ShortNameMatchIgnoresCase() {
        Type? resolved = QuickstartLookup.Resolve("windrunnerquickstart", Candidates, out string? _);

        Assert.AreSame(typeof(Roshar.WindrunnerQuickstart), resolved);
    }

    [TestMethod]
    public void ResolvesANamespacedName() {
        Type? resolved = QuickstartLookup.Resolve(
            typeof(Scadrial.PreCatacendreQuickstart).FullName,
            Candidates,
            out string? error
        );

        Assert.AreSame(typeof(Scadrial.PreCatacendreQuickstart), resolved);
        Assert.IsNull(error);
    }

    [TestMethod]
    public void ResolvesAnAssemblyQualifiedName() {
        Type? resolved = QuickstartLookup.Resolve(
            typeof(Roshar.PreCatacendreQuickstart).AssemblyQualifiedName,
            Candidates,
            out string? error
        );

        Assert.AreSame(typeof(Roshar.PreCatacendreQuickstart), resolved);
        Assert.IsNull(error);
    }

    [TestMethod]
    public void RefusesAShortNameTwoQuickstartsShare() {
        Type? resolved = QuickstartLookup.Resolve("PreCatacendreQuickstart", Candidates, out string? error);

        Assert.IsNull(resolved);
        StringAssert.Contains(error, "matches several quickstarts");
        StringAssert.Contains(error, typeof(Roshar.PreCatacendreQuickstart).FullName);
        StringAssert.Contains(error, typeof(Scadrial.PreCatacendreQuickstart).FullName);
    }

    [TestMethod]
    public void ReportsTheKnownQuickstartsWhenTheNameIsUnknown() {
        Type? resolved = QuickstartLookup.Resolve("NotAQuickstart", Candidates, out string? error);

        Assert.IsNull(resolved);
        StringAssert.Contains(error, "no quickstart is called 'NotAQuickstart'");
        StringAssert.Contains(error, typeof(Roshar.WindrunnerQuickstart).FullName);
    }

    [TestMethod]
    public void TreatsBlankNamesAsNoSelection() {
        Assert.IsNull(QuickstartLookup.Resolve(null, Candidates, out string? nullError));
        Assert.IsNull(QuickstartLookup.Resolve("   ", Candidates, out string? blankError));

        StringAssert.Contains(nullError, "no quickstart name");
        StringAssert.Contains(blankError, "no quickstart name");
    }

    [TestMethod]
    public void TrimsSurroundingWhitespace() {
        Type? resolved = QuickstartLookup.Resolve("  WindrunnerQuickstart  ", Candidates, out string? _);

        Assert.AreSame(typeof(Roshar.WindrunnerQuickstart), resolved);
    }

    // Stand-ins for the real quickstarts, which cannot load outside RimWorld. The two same-named
    // types in different namespaces are what makes the ambiguity case testable.
    private static class Scadrial {
        internal sealed class PreCatacendreQuickstart;
    }

    private static class Roshar {
        internal sealed class PreCatacendreQuickstart;

        internal sealed class WindrunnerQuickstart;
    }
}
