using System;
using UnityEngine;

namespace Cosmere.Core.UI.Skin;

public sealed record SkinTypography(
    string[] TitleFontFamilies,
    string[] BodyFontFamilies,
    int TitlePixelSize = 18,
    int BodyPixelSize = 13,
    float TitleLetterSpacing = 0f,
    bool TitleUpperCase = false
) {
    public static readonly SkinTypography Empty = new SkinTypography(
        Array.Empty<string>(),
        Array.Empty<string>()
    );

    public Font? TitleFont => SkinFontCache.Resolve(TitleFontFamilies, TitlePixelSize);
    public Font? BodyFont => SkinFontCache.Resolve(BodyFontFamilies, BodyPixelSize);
}