
using UnityEngine;
using Verse;
namespace Cosmere.Core.UI.Dock;

public interface IDualInvestiturePair {
    string MetalDefName { get; }
    string MetalLabel { get; }
    Texture2D? MetalIcon { get; }

    float PrimaryMax { get; }
    float PrimaryValue { get; }
    float? PrimaryTarget { get; }
    bool PrimaryActive { get; }
    string PrimaryActionLabel { get; }
    void TogglePrimary();

    float SecondaryMax { get; }
    float SecondaryValue { get; }
    string SecondaryActionLabel { get; }
    void ToggleSecondary();

    string CompoundAbilityDefName { get; }
}
