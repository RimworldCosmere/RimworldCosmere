using System;
using Cosmere;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using Verse;

namespace Cosmere.System.Roshar.LesserSpren.SprenController;

[StaticConstructorOnStartup]
public static class SprenControllerRegistry {
    private static readonly Dictionary<SprenType, BaseSprenController> Controllers =
        new Dictionary<SprenType, BaseSprenController>();

    static SprenControllerRegistry() {
        // Automatically discover and register all BaseSprenController subclasses
        IEnumerable<Type> controllerTypes = typeof(BaseSprenController).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(BaseSprenController)) && !t.IsAbstract);

        foreach (Type? controllerType in controllerTypes) {
            try {
                BaseSprenController? controller = (BaseSprenController)Activator.CreateInstance(controllerType);
                RegisterController(controller);
            } catch (Exception ex) {
                Logger.Error($"Failed to create controller instance for {controllerType.Name}: {ex.Message}");
            }
        }
    }

    public static IEnumerable<BaseSprenController> enabledControllers => Controllers.Values.Where(c => c.isEnabled);

    public static IEnumerable<BaseSprenController> enabledStaticControllers =>
        enabledControllers.Where(v => v is StaticSprenController);

    public static IEnumerable<BaseSprenController> enabledDynamicControllers =>
        enabledControllers.Where(v => v is DynamicSprenController);

    private static void RegisterController(BaseSprenController controller) {
        Controllers[controller.sprenType] = controller;
    }

    public static BaseSprenController? GetController(SprenType sprenType) {
        return Controllers.GetValueOrDefault(sprenType);
    }

    public static bool IsSprenTypeEnabled(SprenType sprenType) {
        BaseSprenController? controller = GetController(sprenType);
        return controller?.isEnabled ?? false;
    }
}