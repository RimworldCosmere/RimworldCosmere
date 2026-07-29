using Cosmere.Core.UI.Skin;
using UnityEngine;

namespace Cosmere.Core.UI.Radial;

public sealed record RadialSystem(
    string SystemId,
    string Label,
    Texture2D? Icon,
    IReadOnlyList<RadialSubsection> Subsections
) {
    // The wheel and the dock's ribbons name and mark a system the same way, because
    // both ask the skin rather than each carrying its own copy of the answer. Spelling
    // the label out at the call site is how the wheel came to call Surgebinding
    // "Stormlight" - the resource rather than the art - long after the ribbon stopped,
    // and how it ended up with no mark at all where the ribbon had one.
    public static RadialSystem ForSystem(string systemId, IReadOnlyList<RadialSubsection> subsections) {
        ISystemSkin skin = SystemSkinRegistry.ForOrFallback(systemId);

        return new RadialSystem(systemId, skin.HeaderLabel, skin.Sigil, subsections);
    }
}
