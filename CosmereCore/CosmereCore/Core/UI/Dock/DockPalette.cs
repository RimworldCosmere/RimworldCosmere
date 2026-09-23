using UnityEngine;

namespace Cosmere.Core.UI.Dock;

public static class DockPalette {
    public static readonly Color Panel = new Color(0.086f, 0.075f, 0.059f);
    public static readonly Color PanelRaised = new Color(0.118f, 0.094f, 0.071f);
    public static readonly Color Border = new Color(0.435f, 0.353f, 0.208f);
    public static readonly Color BorderSubtle = new Color(0.435f, 0.353f, 0.208f, 0.35f);
    public static readonly Color HotLabel = new Color(0.910f, 0.639f, 0.235f);
    public static readonly Color Flare = new Color(0.878f, 0.416f, 0.271f);
    public static readonly Color FlareBorder = new Color(0.784f, 0.314f, 0.180f);
    public static readonly Color GroupLabel = new Color(0.541f, 0.439f, 0.251f);
    public static readonly Color MutedText = new Color(0.659f, 0.604f, 0.502f);

    /// The fill a tile and the strip it opens into both carry - two tones on one merged shape read as
    /// two shapes. Translucent so the parchment grain still shows through, not a flat chip on top.
    public static readonly Color StripFill = new Color(0.063f, 0.051f, 0.039f, 0.38f);

    public const float GhostOpacity = 0.45f;
}
