using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cosmere.Lightweave.Runtime;
using LudeonTK;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Playground;

[StaticConstructorOnStartup]
public static class PlaygroundScreenshotSweep {
    [DebugAction("Cosmere/Core", "Lightweave: Screenshot all playground pages", allowedGameStates = AllowedGameStates.Playing)]
    public static void Run() {
        if (Find.WindowStack.WindowOfType<LightweavePlayground>() == null) {
            Find.WindowStack.Add(new LightweavePlayground());
        }

        SweepDriver.Begin();
    }

    private class SweepDriver : MonoBehaviour {
        private static SweepDriver? _instance;

        public static void Begin() {
            if (_instance != null) {
                Object.Destroy(_instance.gameObject);
                _instance = null;
            }

            GameObject go = new GameObject("LightweavePlaygroundScreenshotSweep");
            Object.DontDestroyOnLoad(go);
            _instance = go.AddComponent<SweepDriver>();
            _instance.StartCoroutine(_instance.Sweep());
        }

        private IEnumerator Sweep() {
            string outDir = Path.Combine(Application.persistentDataPath, "Screenshots", "playground-audit");
            Directory.CreateDirectory(outDir);

            int seq = 0;
            int total = 0;
            for (int i = 0; i < LightweavePlayground.Categories.Count; i++) {
                total += LightweavePlayground.Categories[i].PrimitiveIds.Count;
            }

            LightweaveLog.Message($"screenshot sweep starting: {total} pages -> {outDir}");

            // Close every dialog except the playground itself so the debug-action menu
            // and dev palette don't bleed into the captures.
            List<Window> toClose = new List<Window>();
            foreach (Window w in Find.WindowStack.Windows) {
                if (w is LightweavePlayground) {
                    continue;
                }

                if (w.layer == WindowLayer.Dialog || w.layer == WindowLayer.Super) {
                    toClose.Add(w);
                }
            }

            for (int i = 0; i < toClose.Count; i++) {
                Find.WindowStack.TryRemove(toClose[i], doCloseSound: false);
            }

            yield return null;
            yield return new WaitForEndOfFrame();

            for (int ci = 0; ci < LightweavePlayground.Categories.Count; ci++) {
                PlaygroundCategory cat = LightweavePlayground.Categories[ci];
                for (int pi = 0; pi < cat.PrimitiveIds.Count; pi++) {
                    seq++;
                    string id = cat.PrimitiveIds[pi];
                    LightweavePlayground.OverrideSelectedPrimitive = id;

                    yield return null;
                    yield return null;
                    yield return new WaitForEndOfFrame();

                    string filename = Path.Combine(outDir, $"{seq:D2}-{cat.Id}-{id}.png");
                    ScreenCapture.CaptureScreenshot(filename, 1);
                    LightweaveLog.Message($"sweep {seq}/{total}: {cat.Id}/{id}");

                    yield return new WaitForSeconds(0.45f);
                }
            }

            LightweavePlayground.OverrideSelectedPrimitive = null;
            LightweaveLog.Message($"screenshot sweep complete: {seq} pages -> {outDir}");

            Object.Destroy(gameObject);
            _instance = null;
        }
    }
}
