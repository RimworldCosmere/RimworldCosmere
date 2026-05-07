using Cosmere.Lightweave.Tokens;

namespace Cosmere.Lightweave.Types;

public readonly struct Responsive<T> {
    public readonly T Base;
    public readonly (Breakpoint Bp, T Value)[]? Overrides;

    public Responsive(T @base, (Breakpoint Bp, T Value)[]? overrides = null) {
        Base = @base;
        Overrides = overrides;
    }

    public T Resolve(Breakpoint current) {
        if (Overrides == null || Overrides.Length == 0) return Base;
        T result = Base;
        Breakpoint best = Breakpoint.Xs;
        bool hasMatch = false;
        for (int i = 0; i < Overrides.Length; i++) {
            (Breakpoint bp, T value) = Overrides[i];
            if (bp > current) continue;
            if (!hasMatch || bp >= best) {
                best = bp;
                result = value;
                hasMatch = true;
            }
        }
        return result;
    }

    public static implicit operator Responsive<T>(T value) {
        return new Responsive<T>(value);
    }
}

public static class Responsive {
    public static Responsive<T> Of<T>(
        T @base,
        T? sm = null,
        T? md = null,
        T? lg = null,
        T? xl = null,
        T? xxl = null
    ) where T : struct {
        int count = 0;
        if (sm.HasValue) count++;
        if (md.HasValue) count++;
        if (lg.HasValue) count++;
        if (xl.HasValue) count++;
        if (xxl.HasValue) count++;
        if (count == 0) return new Responsive<T>(@base);

        (Breakpoint, T)[] overrides = new (Breakpoint, T)[count];
        int i = 0;
        if (sm.HasValue) overrides[i++] = (Breakpoint.Sm, sm.Value);
        if (md.HasValue) overrides[i++] = (Breakpoint.Md, md.Value);
        if (lg.HasValue) overrides[i++] = (Breakpoint.Lg, lg.Value);
        if (xl.HasValue) overrides[i++] = (Breakpoint.Xl, xl.Value);
        if (xxl.HasValue) overrides[i++] = (Breakpoint.Xxl, xxl.Value);
        return new Responsive<T>(@base, overrides);
    }

    public static Responsive<T> From<T>(T @base, params (Breakpoint Bp, T Value)[] overrides) {
        return new Responsive<T>(@base, overrides.Length > 0 ? overrides : null);
    }
}
