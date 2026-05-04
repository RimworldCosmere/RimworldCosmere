namespace Cosmere.Core.UI.Radial;

public static class RadialActionRegistry {
    private static readonly List<IRadialActionHandler> handlers = [];

    public static void Register(IRadialActionHandler handler) {
        handlers.Add(handler);
    }

    public static IReadOnlyList<IRadialActionHandler> All => handlers;
}
