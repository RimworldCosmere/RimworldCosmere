using Cosmere.Core.Def;

namespace Cosmere.System.Scadrial.Feruchemy;

public interface IMetalmindSource {
    float StoredAmount { get; }
    float MaxAmount { get; }
    bool CanStore { get; }
    bool CanTap { get; }
    bool Equipped { get; }
    MetalDef? Metal { get; }
    void AddStored(float amount);
    void ConsumeStored(float amount);
}