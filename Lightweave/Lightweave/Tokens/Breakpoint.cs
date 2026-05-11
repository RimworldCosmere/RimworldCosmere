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

    public static Breakpoint Current => Runtime.RenderContext.CurrentOrNull?.Breakpoint ?? Breakpoint.Xs;

    public static T Pick<T>(
        T xs,
        T? sm = null,
        T? md = null,
        T? lg = null,
        T? xl = null,
        T? xxl = null
    ) where T : struct {
        Breakpoint current = Current;
        if (xxl.HasValue && current >= Breakpoint.Xxl) return xxl.Value;
        if (xl.HasValue && current >= Breakpoint.Xl) return xl.Value;
        if (lg.HasValue && current >= Breakpoint.Lg) return lg.Value;
        if (md.HasValue && current >= Breakpoint.Md) return md.Value;
        if (sm.HasValue && current >= Breakpoint.Sm) return sm.Value;
        return xs;
    }

    public static T PickRef<T>(
        T xs,
        T? sm = null,
        T? md = null,
        T? lg = null,
        T? xl = null,
        T? xxl = null
    ) where T : class {
        Breakpoint current = Current;
        if (xxl != null && current >= Breakpoint.Xxl) return xxl;
        if (xl != null && current >= Breakpoint.Xl) return xl;
        if (lg != null && current >= Breakpoint.Lg) return lg;
        if (md != null && current >= Breakpoint.Md) return md;
        if (sm != null && current >= Breakpoint.Sm) return sm;
        return xs;
    }
}
