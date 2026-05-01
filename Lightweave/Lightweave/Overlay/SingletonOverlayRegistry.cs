namespace Cosmere.Lightweave.Overlay;

public static class SingletonOverlayRegistry {
    private static string? activeKey;

    public static void Open(string key) => activeKey = key;

    public static void Close(string key) {
        if (activeKey == key) {
            activeKey = null;
        }
    }

    public static bool ShouldClose(string ownKey) =>
        activeKey != null && activeKey != ownKey;
}
