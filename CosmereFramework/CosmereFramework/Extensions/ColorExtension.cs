using UnityEngine;
using Verse;

namespace CosmereFramework.Extensions;

public static class ColorExtension {
    public static Texture2D ToSolidColorTexture(this Color color) {
        return SolidColorMaterials.NewSolidColorTexture(color);
    }
}