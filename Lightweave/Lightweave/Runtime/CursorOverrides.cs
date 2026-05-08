using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Runtime;

[StaticConstructorOnStartup]
internal static class CursorOverrides {
    private const int DisabledCursorSize = 24;
    private static readonly Vector2 DisabledHotspot = new Vector2(DisabledCursorSize / 2f, DisabledCursorSize / 2f);
    private static int markFrame = -1;
    private static bool cursorOverridden;
    private static Texture2D? cachedDisabledTexture;

    public static void MarkDisabledHover() {
        markFrame = Time.frameCount;
    }

    public static void RestoreDefault() {
        if (Prefs.CustomCursorEnabled) {
            CustomCursor.Activate();
        }
        else {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    public static void ApplyForFrame() {
        if (Event.current == null || Event.current.type != EventType.Repaint) {
            return;
        }

        bool shouldOverride = markFrame == Time.frameCount;

        if (shouldOverride && !cursorOverridden) {
            Texture2D? texture = GetDisabledCursor();
            if (texture != null) {
                Cursor.SetCursor(texture, DisabledHotspot, CursorMode.ForceSoftware);
                cursorOverridden = true;
            }
        }
        else if (!shouldOverride && cursorOverridden) {
            RestoreDefault();
            cursorOverridden = false;
        }
    }

    private static Texture2D? GetDisabledCursor() {
        if (cachedDisabledTexture != null) {
            return cachedDisabledTexture;
        }

        Texture2D source = TexCommand.CannotShoot;
        if (source == null) {
            return null;
        }

        RenderTexture previousActive = RenderTexture.active;
        RenderTexture scratch = RenderTexture.GetTemporary(DisabledCursorSize, DisabledCursorSize, 0, RenderTextureFormat.ARGB32);
        try {
            Graphics.Blit(source, scratch);
            RenderTexture.active = scratch;
            Texture2D output = new Texture2D(DisabledCursorSize, DisabledCursorSize, TextureFormat.ARGB32, mipChain: false, linear: false);
            output.ReadPixels(new Rect(0, 0, DisabledCursorSize, DisabledCursorSize), 0, 0);
            output.Apply();
            cachedDisabledTexture = output;
        }
        finally {
            RenderTexture.active = previousActive;
            RenderTexture.ReleaseTemporary(scratch);
        }

        return cachedDisabledTexture;
    }
}
