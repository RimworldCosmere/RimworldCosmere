namespace Cosmere.Core.Nightwatcher;

public interface INightwatcherEffectDescriber {
    string? DescribeEffects(NightwatcherApplicationContext? context = null);
}
