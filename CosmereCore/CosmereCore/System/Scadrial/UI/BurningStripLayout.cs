namespace Cosmere.System.Scadrial.UI;

// Deliberately free of RimWorld and Unity types so the test host can load it.
public static class BurningStripLayout {
    public const float HeaderHeight = 18f;
    public const float RowHeight = 20f;
    public const float Padding = 6f;
    public const float Footer = 6f;

    /// Nothing burning costs no height at all, so the table sits straight under the heading.
    public static float HeightFor(int rows) {
        return rows <= 0 ? 0f : HeaderHeight + Padding + rows * RowHeight + Footer;
    }
}
