using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CosmereFramework.Comp.Map;

public abstract class Renderer<T>(Verse.Map map) : MapComponent(map) {
    public static Dictionary<object, HashSet<T>> ToRender = [];

    public override void MapComponentUpdate() {
        foreach (T item in ToRender.Keys.SelectMany(key => ToRender[key])) {
            RenderItem(item);
        }
    }

    protected abstract void RenderItem(T item);

    public static void TryAdd(object source, T item) {
        if (!ToRender.ContainsKey(source)) {
            ToRender[source] = new HashSet<T>();
        }

        ToRender[source].Add(item);
    }

    public static void TryClear(object source) {
        if (ToRender.ContainsKey(source)) {
            ToRender[source].Clear();
        }
    }

    public static void TryRemove(object source) {
        ToRender.Remove(source);
    }
}