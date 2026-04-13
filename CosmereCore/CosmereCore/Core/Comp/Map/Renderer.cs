using Verse;

namespace Cosmere.Core.Comp.Map;

public abstract class Renderer<T>(Verse.Map map) : MapComponent(map) {
    private static Dictionary<object, HashSet<T>> ToRender = [];

    public override void MapComponentUpdate() {
        foreach (T item in ToRender.Keys.SelectMany(key => ToRender[key])) {
            RenderItem(item);
        }
    }

    protected abstract void RenderItem(T item);

    public static void TryAdd(object source, T item) {
        if (!ToRender.ContainsKey(source)) {
            ToRender[source] = [];
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