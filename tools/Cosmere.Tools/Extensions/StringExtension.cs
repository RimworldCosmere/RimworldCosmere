namespace Cosmere.Tools.Extensions;

public static class StringExtension {
    public static string ToDefName(this string input) {
        return input.ToTitleCase().Replace(" ", string.Empty);
    }

    public static string ToTitleCase(this string input) {
        if (string.IsNullOrEmpty(input)) return input;

        return string.Join(" ", input.ToLowerInvariant().Split(' ')
            .Select(word => word.Length > 0 ? char.ToUpper(word[0]) + word[1..] : word));
    }
}
