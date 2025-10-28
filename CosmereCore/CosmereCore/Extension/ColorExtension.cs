using UnityEngine;
using Verse;

namespace Cosmere.Extension;

public static class ColorExtension {
    public static Texture2D ToSolidColorTexture(this Color color) {
        return SolidColorMaterials.NewSolidColorTexture(color);
    }
}