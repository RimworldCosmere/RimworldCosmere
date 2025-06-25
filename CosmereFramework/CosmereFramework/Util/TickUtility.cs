namespace CosmereFramework.Util;

public static class TickUtility {
    public const float TicksPerMillisecond = TicksPerSecond / 1000;
    public const float TicksPerSecond = 60f;
    public const float TicksPerMinute = TicksPerSecond * 60;
    public const float TicksPerHour = TicksPerMinute * 60;
    public const float TicksPerDay = 60000; // RimWorld defines 1 day = 60,000 ticks

    public static float Milliseconds(int milliseconds) {
        return Milliseconds((float)milliseconds);
    }

    public static float Milliseconds(float milliseconds) {
        return milliseconds * TicksPerMillisecond;
    }

    public static float Seconds(int seconds) {
        return Seconds((float)seconds);
    }

    public static float Seconds(float seconds) {
        return seconds * TicksPerSecond;
    }

    public static float Minutes(int minutes) {
        return Minutes((float)minutes);
    }

    public static float Minutes(float minutes) {
        return minutes * TicksPerMinute;
    }

    public static float Hours(float hours) {
        return hours * TicksPerHour;
    }

    public static float Days(int days) {
        return Days((float)days);
    }

    public static float Days(float days) {
        return days * TicksPerDay;
    }

    public static float ToSeconds(int ticks) {
        return ticks / TicksPerSecond;
    }

    public static float ToMinutes(int ticks) {
        return ticks / TicksPerMinute;
    }

    public static float ToHours(int ticks) {
        return ticks / TicksPerHour;
    }

    public static float ToDays(int ticks) {
        return ticks / TicksPerDay;
    }

    public static string FormatTicksAsTime(int ticks) {
        float totalSeconds = ticks / TicksPerSecond;
        int minutes = (int)(totalSeconds / 60);
        int seconds = (int)(totalSeconds % 60);
        return $"{minutes:D2}:{seconds:D2}";
    }
}