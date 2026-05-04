using System;

namespace Cosmere.Lightweave.Input;

public sealed record NumberFieldOptions(
    Func<string, float?>? Parse = null,
    Func<float, string>? Format = null,
    bool AllowDecimal = true,
    int DecimalPlaces = 2
);
