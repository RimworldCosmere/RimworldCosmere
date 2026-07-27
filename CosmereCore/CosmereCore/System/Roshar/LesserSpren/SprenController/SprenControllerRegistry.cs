using Cosmere.System.Roshar.LesserSpren.ParticleSystem;

namespace Cosmere.System.Roshar.LesserSpren.SprenController;

public static class SprenControllerRegistry {
    private static readonly Dictionary<SprenType, BaseSprenController> Controllers =
        new Dictionary<SprenType, BaseSprenController>();

    private static List<BaseSprenController> cachedEnabled = [];
    private static List<BaseSprenController> cachedStatic = [];
    private static List<BaseSprenController> cachedDynamic = [];

    public static IReadOnlyList<BaseSprenController> enabledControllers => cachedEnabled;

    public static IReadOnlyList<BaseSprenController> enabledStaticControllers => cachedStatic;

    public static IReadOnlyList<BaseSprenController> enabledDynamicControllers => cachedDynamic;

    public static void Register(BaseSprenController controller) {
        Controllers[controller.sprenType] = controller;
        RebuildCaches();
    }

    public static BaseSprenController? GetController(SprenType sprenType) {
        return Controllers.GetValueOrDefault(sprenType);
    }

    public static bool IsSprenTypeEnabled(SprenType sprenType) {
        BaseSprenController? controller = GetController(sprenType);
        return controller?.isEnabled ?? false;
    }

    private static void RebuildCaches() {
        cachedEnabled = [];
        cachedStatic = [];
        cachedDynamic = [];
        foreach (BaseSprenController c in Controllers.Values) {
            if (!c.isEnabled) continue;
            cachedEnabled.Add(c);
            if (c is StaticSprenController) cachedStatic.Add(c);
            else if (c is DynamicSprenController) cachedDynamic.Add(c);
        }
    }
}
