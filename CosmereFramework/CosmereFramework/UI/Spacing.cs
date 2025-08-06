namespace Cosmere.Framework.UI;

public class Spacing {
    public const int BaseUnit = 16;

    public static float Get() {
        return BaseUnit;
    }

    public static float Get(float multiplier) {
        return Get() * multiplier;
    }

    public static float Get(double multiplier) {
        return Get((float)multiplier);
    }
}