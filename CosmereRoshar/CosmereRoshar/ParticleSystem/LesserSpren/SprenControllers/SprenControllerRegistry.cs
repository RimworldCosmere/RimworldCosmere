using System;
using Cosmere.Foundation;
using Verse;

namespace Cosmere.Roshar.ParticleSystem.LesserSpren.SprenControllers;

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

    private static void RegisterController(BaseSprenController controller) {
        Controllers[controller.sprenType] = controller;
    }

    public static BaseSprenController GetController(SprenType sprenType) {
        return Controllers.TryGetValue(sprenType, out BaseSprenController controller) ? controller : null;
    }

    public static bool IsSprenTypeEnabled(SprenType sprenType) {
        BaseSprenController controller = GetController(sprenType);
        return controller?.isEnabled ?? false;
    }

    public static IEnumerable<BaseSprenController> GetEnabledControllers() {
        return Controllers.Values.Where(v => v.isEnabled);
    }

    public static IEnumerable<BaseSprenController> GetEnabledNatureControllers() {
        return Controllers.Values.Where(v => v.isEnabled && v.isNatureSpren);
    }

    public static IEnumerable<BaseSprenController> GetEnabledDynamicControllers() {
        return Controllers.Values.Where(v => v.isEnabled && !v.isNatureSpren);
    }
}