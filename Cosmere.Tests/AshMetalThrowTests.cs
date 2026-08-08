using Cosmere.System.Scadrial.Grid;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

[TestClass]
public class AshMetalThrowTests {
    [TestMethod]
    public void SubThrowFractionsBankRatherThanVanish() {
        float remainder = 0f;
        int total = 0;
        for (int i = 0; i < 1000; i++) {
            total += AshMetalThrow.Bank(ref remainder, 0.5f, 0.1f);
        }

        Assert.IsTrue(total >= 49 && total <= 50, $"banked {total} throws, wanted 49-50");
    }

    [TestMethod]
    public void ABankedThrowIsNeverLostToRounding() {
        float remainder = 0f;
        int total = 0;
        for (int i = 0; i < 50_000; i++) {
            total += AshMetalThrow.Bank(ref remainder, 0.5f, 0.02f);
        }

        Assert.IsTrue(total >= 499 && total <= 500, $"banked {total} throws, wanted 499-500");
    }

    [TestMethod]
    public void MetalChoiceIsStableForTheSameThrowAndSeed() {
        Assert.AreEqual(
            AshMetalThrow.MetalIndex(7, 12345, 4),
            AshMetalThrow.MetalIndex(7, 12345, 4)
        );
    }

    [TestMethod]
    public void MetalChoiceSpreadsAcrossEveryMetal() {
        bool[] seen = new bool[4];
        for (int i = 0; i < 400; i++) {
            seen[AshMetalThrow.MetalIndex(i, 999, 4)] = true;
        }

        for (int i = 0; i < 4; i++) {
            Assert.IsTrue(seen[i], $"metal {i} never came up");
        }
    }

    [TestMethod]
    public void MetalIndexIsAlwaysInRange() {
        for (int i = 0; i < 500; i++) {
            int index = AshMetalThrow.MetalIndex(i, i * 31, 4);
            Assert.IsTrue(index >= 0 && index <= 3, $"throw {i} gave index {index}");
        }
    }
}
