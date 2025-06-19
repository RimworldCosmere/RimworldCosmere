using UnityEngine;

namespace CosmereFramework.Utils;

public static class TextureUtility {
    public static void InvertColors(Texture2D texture) {
        if (!texture.isReadable) {
            Debug.LogError("Texture is not readable. Set it to readable in the import settings or clone it manually.");
            return;
        }

        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++) {
            Color c = pixels[i];
            pixels[i] = new Color(1f - c.r, 1f - c.g, 1f - c.b, c.a); // invert RGB, keep alpha
        }

        texture.SetPixels(pixels);
        texture.Apply();
    }

    public static Texture2D CloneTexture(Texture2D original) {
        RenderTexture rt = RenderTexture.GetTemporary(original.width, original.height);
        Graphics.Blit(original, rt);

        RenderTexture.active = rt;
        Texture2D readable = new Texture2D(original.width, original.height);
        readable.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        readable.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return readable;
    }
}