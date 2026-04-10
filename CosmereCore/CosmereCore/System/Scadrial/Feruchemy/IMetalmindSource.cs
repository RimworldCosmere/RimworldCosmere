namespace Cosmere.System.Scadrial.Feruchemy;

public interface IMetalmindSource {
    float storedAmount { get; }
    float maxAmount { get; }
    bool canStore { get; }
    bool canTap { get; }
    bool equipped { get; }
    Cosmere.Core.Def.MetalDef metal { get; }
    void AddStored(float amount);
    void ConsumeStored(float amount);
}
