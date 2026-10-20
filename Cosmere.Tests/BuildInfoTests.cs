using System;
using System.Globalization;
using Cosmere.Core.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     BuildInfo reads the assembly, so a build that forgets to stamp either value would ship
///     "unknown" to the BetaHub bundle and the beta gate.
/// </summary>
[TestClass]
public class BuildInfoTests {
    [TestMethod]
    public void RevisionComesFromTheAssembly() {
        Assert.AreNotEqual("unknown", BuildInfo.Revision);
        Assert.IsFalse(string.IsNullOrWhiteSpace(BuildInfo.Revision));
    }

    [TestMethod]
    public void BuildTimeIsAnIsoTimestamp() {
        Assert.IsTrue(
            DateTime.TryParse(BuildInfo.BuildTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _),
            $"BuildTime was '{BuildInfo.BuildTime}'"
        );
    }
}
