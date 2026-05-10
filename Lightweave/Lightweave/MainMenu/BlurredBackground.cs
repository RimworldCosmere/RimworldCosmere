using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.MainMenu;

[StaticConstructorOnStartup]
public static class BlurredBackground {
    private const float TopScrimAlpha = 0.18f;
    private const float BottomScrimAlpha = 0.55f;
    private const float CenterDarkenAlpha = 0.32f;

    private static readonly Texture2D Scrim = SolidColorMaterials.NewSolidColorTexture(new Color(0f, 0f, 0f, 1f));
    private static readonly Texture2D ScrimGradient = SolidColorMaterials.NewSolidColorTexture(new Color(0.04f, 0.05f, 0.07f, 1f));

    public static void Draw(Rect screen) {
        if (Event.current.type != EventType.Repaint) {
            return;
        }

        Color prev = GUI.color;

        GUI.color = new Color(1f, 1f, 1f, CenterDarkenAlpha);
        GUI.DrawTexture(screen, Scrim);

        Rect topBand = new Rect(screen.x, screen.y, screen.width, screen.height * 0.18f);
        Rect bottomBand = new Rect(screen.x, screen.yMax - screen.height * 0.32f, screen.width, screen.height * 0.32f);

        GUI.color = new Color(1f, 1f, 1f, TopScrimAlpha);
        GUI.DrawTexture(topBand, ScrimGradient);

        GUI.color = new Color(1f, 1f, 1f, BottomScrimAlpha);
        GUI.DrawTexture(bottomBand, ScrimGradient);

        GUI.color = prev;
    }
}
