using System;
using System.Reflection;
using Cosmere.Core.BetaHub;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Guards the JSON string decoder that reads BetaHub issue bodies. A loop that forgets to
///     step past the escaped char emits it twice, which corrupts every multi-line report.
/// </summary>
[TestClass]
public class BetaHubUnescapeTests {
    private static readonly MethodInfo Method = typeof(BetaHubClient).GetMethod(
        "Unescape",
        BindingFlags.NonPublic | BindingFlags.Static
    ) ?? throw new InvalidOperationException("BetaHubClient.Unescape is gone");

    private static string Unescape(string value) {
        return (string)Method.Invoke(null, [value])!;
    }

    [TestMethod]
    public void PlainTextPassesThrough() {
        Assert.AreEqual("a b c", Unescape("a b c"));
        Assert.AreEqual(string.Empty, Unescape(string.Empty));
    }

    [TestMethod]
    public void EscapeTableDecodesOnce() {
        Assert.AreEqual("a\nb", Unescape("a\\nb"));
        Assert.AreEqual("a\rb", Unescape("a\\rb"));
        Assert.AreEqual("a\tb", Unescape("a\\tb"));
        Assert.AreEqual("a\bb", Unescape("a\\bb"));
        Assert.AreEqual("a\fb", Unescape("a\\fb"));
    }

    [TestMethod]
    public void UnknownEscapeKeepsTheTargetChar() {
        Assert.AreEqual("a\"b", Unescape("a\\\"b"));
        Assert.AreEqual("a\\b", Unescape("a\\\\b"));
        Assert.AreEqual("a/b", Unescape("a\\/b"));
    }

    [TestMethod]
    public void MultiLineBodySurvives() {
        Assert.AreEqual("line one\nline two\nline three", Unescape("line one\\nline two\\nline three"));
    }

    [TestMethod]
    public void TrailingBackslashIsKept() {
        Assert.AreEqual("a\\", Unescape("a\\"));
    }
}
