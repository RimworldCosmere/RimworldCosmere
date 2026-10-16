using Cosmere.System.Roshar.LesserSpren.ParticleSystem;

namespace Cosmere.System.Roshar.LesserSpren.SprenController;

public static class SprenControllerRegistry {
    private static readonly Dictionary<SprenType, BaseSprenController> Controllers = [];

    private static List<BaseSprenController> cachedEnabled = [];
    private static List<BaseSprenController> cachedStatic = [];
    private static List<BaseSprenController> cachedDynamic = [];

    public static IReadOnlyList<BaseSprenController> EnabledControllers => cachedEnabled;

    public static IReadOnlyList<BaseSprenController> EnabledStaticControllers => cachedStatic;

    public static IReadOnlyList<BaseSprenController> EnabledDynamicControllers => cachedDynamic;

    public static void Register(BaseSprenController controller) {
        Controllers[controller.SprenType] = controller;
        RebuildCaches();
    }

    public static BaseSprenController? GetController(SprenType sprenType) {
        return Controllers.GetValueOrDefault(sprenType);
    }

    public static bool IsSprenTypeEnabled(SprenType sprenType) {
        BaseSprenController? controller = GetController(sprenType);
        return controller?.IsEnabled ?? false;
    }

    private static void RebuildCaches() {
        cachedEnabled = [];
        cachedStatic = [];
        cachedDynamic = [];
        foreach (BaseSprenController c in Controllers.Values) {
            if (!c.IsEnabled) continue;
            cachedEnabled.Add(c);
            if (c is StaticSprenController) cachedStatic.Add(c);
            else if (c is DynamicSprenController) cachedDynamic.Add(c);
        }
    }
}
