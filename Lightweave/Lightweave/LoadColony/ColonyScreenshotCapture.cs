using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cosmere.Lightweave.Runtime;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.LoadColony;

internal static class ColonyScreenshotCapture {
    public const int OutWidth = 480;
    public const int OutHeight = 300;

    public static void ScheduleForSave(string saveFilePath) {
        if (string.IsNullOrEmpty(saveFilePath)) {
            return;
        }
        Map? map = Find.CurrentMap;
        if (map == null) {
            return;
        }
        Driver.Begin(saveFilePath, map);
    }

    private class Driver : MonoBehaviour {
        private string _saveFilePath = string.Empty;
        private Map? _map;

        public static void Begin(string saveFilePath, Map map) {
            GameObject go = new GameObject("LightweaveColonyScreenshot");
            DontDestroyOnLoad(go);
            Driver d = go.AddComponent<Driver>();
            d._saveFilePath = saveFilePath;
            d._map = map;
            d.StartCoroutine(d.Capture());
        }

        private IEnumerator Capture() {
            yield return null;

            CameraDriver? camDriver = Find.CameraDriver;
            Camera? cam = Find.Camera;
            ScreenshotModeHandler? screenshotMode = Find.UIRoot?.screenshotMode;
            WindowStack? windowStack = Find.WindowStack;
            if (camDriver == null || cam == null || _map == null) {
                Cleanup();
                yield break;
            }

            Vector3 prevPos = camDriver.transform.position;
            float prevSize = camDriver.RootSize;
            bool prevScreenshotMode = screenshotMode != null && screenshotMode.Active;
            float prevMaxSize = camDriver.config.sizeRange.max;

            List<Window> hidden = new List<Window>();
            if (windowStack != null) {
                List<Window> snapshot = new List<Window>(windowStack.Windows);
                for (int i = 0; i < snapshot.Count; i++) {
                    Window w = snapshot[i];
                    if (w == null) continue;
                    if (w.layer == WindowLayer.GameUI) continue;
                    hidden.Add(w);
                }
                for (int i = 0; i < hidden.Count; i++) {
                    try {
                        windowStack.TryRemove(hidden[i], doCloseSound: false);
                    }
                    catch {
                    }
                }
            }

            Texture2D? raw = null;
            string base64 = string.Empty;
            try {
                if (TryComputeColonyView(_map, OutWidth, OutHeight, out Vector3 center, out float orthoSize)) {
                    if (orthoSize > prevMaxSize) {
                        camDriver.config.sizeRange.max = orthoSize + 1f;
                    }
                    camDriver.SetRootPosAndSize(center, orthoSize);
                }

                if (screenshotMode != null) {
                    screenshotMode.Active = true;
                }
            }
            catch {
            }

            yield return null;
            yield return new WaitForEndOfFrame();

            try {
                raw = ScreenCapture.CaptureScreenshotAsTexture();
                if (raw != null) {
                    base64 = CropAndScale(raw, OutWidth, OutHeight);
                }

                if (!string.IsNullOrEmpty(base64)) {
                    SaveSidecar.UpdateScreenshot(_saveFilePath, base64);
                }
            }
            finally {
                if (raw != null) {
                    UnityEngine.Object.Destroy(raw);
                }
                if (screenshotMode != null) {
                    screenshotMode.Active = prevScreenshotMode;
                }
                camDriver.config.sizeRange.max = prevMaxSize;
                camDriver.SetRootPosAndSize(prevPos, prevSize);
                Cleanup();
            }
        }

        private void Cleanup() {
            if (gameObject != null) {
                Destroy(gameObject);
            }
        }
    }

    private static bool TryComputeColonyView(Map map, int outW, int outH, out Vector3 center, out float orthoSize) {
        center = Vector3.zero;
        orthoSize = 0f;

        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minZ = int.MaxValue;
        int maxZ = int.MinValue;
        int count = 0;

        List<Building>? buildings = map.listerBuildings?.allBuildingsColonist;
        if (buildings != null) {
            for (int i = 0; i < buildings.Count; i++) {
                IntVec3 p = buildings[i].Position;
                if (p.x < minX) minX = p.x;
                if (p.x > maxX) maxX = p.x;
                if (p.z < minZ) minZ = p.z;
                if (p.z > maxZ) maxZ = p.z;
                count++;
            }
        }

        IReadOnlyList<Pawn>? colonists = map.mapPawns?.FreeColonists;
        if (colonists != null) {
            for (int i = 0; i < colonists.Count; i++) {
                IntVec3 p = colonists[i].Position;
                if (p.x < minX) minX = p.x;
                if (p.x > maxX) maxX = p.x;
                if (p.z < minZ) minZ = p.z;
                if (p.z > maxZ) maxZ = p.z;
                count++;
            }
        }

        if (count == 0) {
            return false;
        }

        const int padding = 12;
        minX = Mathf.Max(0, minX - padding);
        maxX = Mathf.Min(map.Size.x - 1, maxX + padding);
        minZ = Mathf.Max(0, minZ - padding);
        maxZ = Mathf.Min(map.Size.z - 1, maxZ + padding);

        float cx = (minX + maxX + 1) * 0.5f;
        float cz = (minZ + maxZ + 1) * 0.5f;
        center = new Vector3(cx, 0f, cz);

        float spanX = maxX - minX + 1;
        float spanZ = maxZ - minZ + 1;
        float aspect = (float)outW / outH;
        float fitFromHeight = spanZ * 0.5f;
        float fitFromWidth = spanX * 0.5f / aspect;
        orthoSize = Mathf.Max(fitFromHeight, fitFromWidth);
        orthoSize = Mathf.Max(orthoSize, 11f);
        return true;
    }

    private static string CropAndScale(Texture2D src, int outW, int outH) {
        int srcW = src.width;
        int srcH = src.height;
        if (srcW == 0 || srcH == 0) {
            return string.Empty;
        }

        float srcAspect = (float)srcW / srcH;
        float dstAspect = (float)outW / outH;

        int cropW;
        int cropH;
        int cropX;
        int cropY;
        if (srcAspect > dstAspect) {
            cropH = srcH;
            cropW = Mathf.RoundToInt(srcH * dstAspect);
            cropX = (srcW - cropW) / 2;
            cropY = 0;
        }
        else {
            cropW = srcW;
            cropH = Mathf.RoundToInt(srcW / dstAspect);
            cropX = 0;
            cropY = (srcH - cropH) / 2;
        }

        RenderTexture? rt = null;
        Texture2D? scaled = null;
        RenderTexture? prevActive = RenderTexture.active;
        try {
            rt = RenderTexture.GetTemporary(outW, outH, 0, RenderTextureFormat.ARGB32);
            Vector2 scale = new Vector2((float)cropW / srcW, (float)cropH / srcH);
            Vector2 offset = new Vector2((float)cropX / srcW, (float)cropY / srcH);
            Graphics.Blit(src, rt, scale, offset);

            RenderTexture.active = rt;
            scaled = new Texture2D(outW, outH, TextureFormat.RGB24, false);
            scaled.ReadPixels(new Rect(0, 0, outW, outH), 0, 0);
            scaled.Apply();

            byte[] png = scaled.EncodeToPNG();
            return Convert.ToBase64String(png);
        }
        finally {
            RenderTexture.active = prevActive;
            if (rt != null) {
                RenderTexture.ReleaseTemporary(rt);
            }
            if (scaled != null) {
                UnityEngine.Object.Destroy(scaled);
            }
        }
    }
}
