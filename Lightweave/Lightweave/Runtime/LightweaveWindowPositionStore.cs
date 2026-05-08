using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Runtime;

public sealed class LightweaveWindowPositionStore : GameComponent {
    private List<StoredRect> stored = new List<StoredRect>();

    public LightweaveWindowPositionStore(Game game) { }

    public static LightweaveWindowPositionStore? GetOrNull() {
        if (Current.Game == null) {
            return null;
        }

        return Current.Game.GetComponent<LightweaveWindowPositionStore>();
    }

    public bool TryGet(string key, out Rect rect) {
        for (int i = 0; i < stored.Count; i++) {
            StoredRect s = stored[i];
            if (s.Key == key) {
                rect = new Rect(s.X, s.Y, s.W, s.H);
                return true;
            }
        }

        rect = default;
        return false;
    }

    public void Set(string key, Rect rect) {
        for (int i = 0; i < stored.Count; i++) {
            if (stored[i].Key == key) {
                stored[i].X = rect.x;
                stored[i].Y = rect.y;
                stored[i].W = rect.width;
                stored[i].H = rect.height;
                return;
            }
        }

        stored.Add(new StoredRect {
            Key = key,
            X = rect.x,
            Y = rect.y,
            W = rect.width,
            H = rect.height,
        });
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Collections.Look(ref stored, "stored", LookMode.Deep);
        if (Scribe.mode == LoadSaveMode.PostLoadInit && stored == null) {
            stored = new List<StoredRect>();
        }
    }

    private sealed class StoredRect : IExposable {
        public string Key = string.Empty;
        public float X;
        public float Y;
        public float W;
        public float H;

        public void ExposeData() {
            Scribe_Values.Look(ref Key, "key", string.Empty);
            Scribe_Values.Look(ref X, "x");
            Scribe_Values.Look(ref Y, "y");
            Scribe_Values.Look(ref W, "w");
            Scribe_Values.Look(ref H, "h");
        }
    }
}
