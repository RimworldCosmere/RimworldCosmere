namespace Cosmere.Lightweave.Tokens;

public enum Breakpoint {
    Xs = 0,
    Sm = 1,
    Md = 2,
    Lg = 3,
    Xl = 4,
    Xxl = 5,
}

public static class Breakpoints {
    public const float SmMinPx = 640f;
    public const float MdMinPx = 768f;
    public const float LgMinPx = 1024f;
    public const float XlMinPx = 1280f;
    public const float XxlMinPx = 1536f;

    public static Breakpoint For(float widthPx) {
        if (widthPx >= XxlMinPx) return Breakpoint.Xxl;
        if (widthPx >= XlMinPx) return Breakpoint.Xl;
        if (widthPx >= LgMinPx) return Breakpoint.Lg;
        if (widthPx >= MdMinPx) return Breakpoint.Md;
        if (widthPx >= SmMinPx) return Breakpoint.Sm;
        return Breakpoint.Xs;
    }
}
