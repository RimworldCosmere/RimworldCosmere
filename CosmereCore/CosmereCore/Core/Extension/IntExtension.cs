namespace Cosmere.Core.Extension;

public static class IntExtension {
    public static string ToOrdinal(this int number, bool shortForm = false) {
        return shortForm ? number.ToOrdinalShort() : number.ToOrdinalLong();
    }

    public static string ToOrdinalLong(this int number) {
        return number switch {
            <= 0 => number.ToString(),
            <= 10 => number switch {
                1 => "first",
                2 => "second",
                3 => "third",
                4 => "fourth",
                5 => "fifth",
                6 => "sixth",
                7 => "seventh",
                8 => "eighth",
                9 => "ninth",
                10 => "tenth",
            },
            _ => number.ToOrdinalShort(),
        };
    }

    public static string ToOrdinalShort(this int number) {
        if (number <= 0) return number.ToString();

        switch (number % 100) {
            case 11:
            case 12:
            case 13:
                return number + "th";
        }

        switch (number % 10) {
            case 1:
                return number + "st";
            case 2:
                return number + "nd";
            case 3:
                return number + "rd";
            default:
                return number + "th";
        }
    }
}