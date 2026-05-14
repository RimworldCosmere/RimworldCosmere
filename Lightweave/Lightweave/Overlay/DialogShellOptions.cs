using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Lightweave.Overlay;

internal struct DialogShellOptions {
    public bool IsModal;
    public float? Width;
    public float? Height;
    public float WidthFraction;
    public float HeightFraction;
    public float MaxWidth;
    public float MaxHeight;
    public Color? ScrimColor;
    public BackgroundSpec? CardBackground;
    public BorderSpec? CardBorder;
    public EdgeInsets? CardPadding;
    public RadiusSpec? CardRadius;
    public bool DrawGradient;
    public Color? GradientTopColor;
    public Color? GradientBottomColor;
    public bool DrawVignette;
    public VignetteShape VignetteShape;
    public float VignetteIntensity;
    public float VignetteScale;
    public ColorRef? VignetteColor;
}
