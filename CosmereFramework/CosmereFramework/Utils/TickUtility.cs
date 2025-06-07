namespace CosmereFramework.Utils {
    public static class TickUtility {
        public const float TICKS_PER_MILLISECOND = TICKS_PER_SECOND / 1000;
        public const float TICKS_PER_SECOND = 60f;
        public const float TICKS_PER_MINUTE = TICKS_PER_SECOND * 60;
        public const float TICKS_PER_HOUR = TICKS_PER_MINUTE * 60;
        public const float TICKS_PER_DAY = 60000; // RimWorld defines 1 day = 60,000 ticks

        public static float Milliseconds(int milliseconds) {
            return Milliseconds((float)milliseconds);
        }

        public static float Milliseconds(float milliseconds) {
            return milliseconds * TICKS_PER_MILLISECOND;
        }

        public static float Seconds(int seconds) {
            return Seconds((float)seconds);
        }

        public static float Seconds(float seconds) {
            return seconds * TICKS_PER_SECOND;
        }

        public static float Minutes(int minutes) {
            return Minutes((float)minutes);
        }

        public static float Minutes(float minutes) {
            return minutes * TICKS_PER_MINUTE;
        }

        public static float Hours(float hours) {
            return hours * TICKS_PER_HOUR;
        }

        public static float Days(int days) {
            return Days((float)days);
        }

        public static float Days(float days) {
            return days * TICKS_PER_DAY;
        }

        public static float ToSeconds(int ticks) {
            return ticks / TICKS_PER_SECOND;
        }

        public static float ToMinutes(int ticks) {
            return ticks / TICKS_PER_MINUTE;
        }

        public static float ToHours(int ticks) {
            return ticks / TICKS_PER_HOUR;
        }

        public static float ToDays(int ticks) {
            return ticks / TICKS_PER_DAY;
        }

        public static string FormatTicksAsTime(int ticks) {
            var totalSeconds = ticks / TICKS_PER_SECOND;
            var minutes = (int)(totalSeconds / 60);
            var seconds = (int)(totalSeconds % 60);
            return $"{minutes:D2}:{seconds:D2}";
        }
    }
}