using System;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Extension;

public static class TextureExtension {
    public static Texture2D Overlay(this Texture2D orig, Texture2D overlay, float alpha = 1f) {
        return MakeTexture(orig.width, orig.height, Draw);

        void Draw(RenderTexture render) {
            Graphics.Blit(orig, render);

            Material mat = MaterialPool.MatFrom(
                overlay,
                Verse.ShaderDatabase.MetaOverlay,
                new Color(1f, 1f, 1f, alpha)
            );
            Graphics.Blit(overlay, render, mat);
        }
    }

    public static Texture2D Transform(this Texture2D orig, Rect pos, IntVec2 imageSize = default) {
        if (imageSize == default) {
            imageSize = new IntVec2(orig.width, orig.height);
        }

        return MakeTexture(imageSize.x, imageSize.z, Draw);

        void Draw(RenderTexture render) {
            Vector2 origSize = new Vector2(orig.width, orig.height);
            Vector2 scale = imageSize.ToVector2() / origSize / pos.size;
            Vector2 offset = new Vector2(-pos.x, -pos.yMax) * scale;
            Graphics.Blit(orig, render, scale, offset);
        }
    }

    private static Texture2D MakeTexture(int width, int height, Action<RenderTexture> draw) {
        RenderTexture? render = RenderTexture.GetTemporary(
            width,
            height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear
        );

        draw(render);

        RenderTexture.active = render;
        Texture2D res = new Texture2D(width, height);
        res.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        res.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(render);
        return res;
    }

    public static Texture2D InvertColors(this Texture2D texture) {
        if (!texture.isReadable) {
            Logger.Error("Texture is not readable. Set it to readable in the import settings or clone it manually.");
            return texture;
        }

        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++) {
            Color c = pixels[i];
            pixels[i] = new Color(1f - c.r, 1f - c.g, 1f - c.b, c.a);
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return texture;
    }

    /// <summary>
    ///     Flattens every pixel to <paramref name="color" /> while keeping the original alpha, giving a
    ///     solid-colour silhouette of the artwork.
    /// </summary>
    /// <remarks>
    ///     Needed because GUI.color multiplies: a glyph that is already painted red cannot be drawn
    ///     white by tinting it, only darker. Recolouring the pixels is the only way.
    /// </remarks>
    public static Texture2D Silhouette(this Texture2D texture, Color color) {
        if (!texture.isReadable) {
            Logger.Error("Texture is not readable. Set it to readable in the import settings or clone it manually.");
            return texture;
        }

        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++) {
            pixels[i] = new Color(color.r, color.g, color.b, pixels[i].a);
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return texture;
    }

    public static Texture2D CloneTexture(this Texture2D original) {
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
