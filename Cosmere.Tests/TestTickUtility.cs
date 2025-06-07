using CosmereFramework.Utils;

namespace Cosmere.Tests {
    [TestClass]
    public class TickUtilityTests {
        [TestMethod]
        public void SecondsToTicks_Is60TimesInput() {
            Assert.AreEqual(120f, TickUtility.Seconds(2));
        }

        [TestMethod]
        public void FormatTicksAsTime_FormatsAsMinutesAndSeconds() {
            Assert.AreEqual("02:20", TickUtility.FormatTicksAsTime(8400));
        }
    }
}