using Cosmere.Core.UI.Skin;
using UnityEngine;

namespace Cosmere.Core.UI.Radial;

public sealed record RadialSystem(
    string SystemId,
    string Label,
    Texture2D? Icon,
    IReadOnlyList<RadialSubsection> Subsections
) {
    /// <summary>
    ///     Wheel and dock ribbons name and mark a system the same way by asking the skin, not carrying
    ///     their own copy - a duplicated copy once drifted, mislabeling Surgebinding as "Stormlight".
    /// </summary>
    /// <returns>A system entry carrying the header label and sigil the system's skin supplies.</returns>
    public static RadialSystem ForSystem(string systemId, IReadOnlyList<RadialSubsection> subsections) {
        ISystemSkin skin = SystemSkinRegistry.ForOrFallback(systemId);

        return new RadialSystem(systemId, skin.HeaderLabel, skin.Sigil, subsections);
    }
}
