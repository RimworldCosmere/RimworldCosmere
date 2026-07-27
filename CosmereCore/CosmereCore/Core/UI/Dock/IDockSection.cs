using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Dock;

public interface IDockSection {
    string SystemId { get; }

    ISystemSkin Skin { get; }

    float GetHeaderHeight();

    float GetExpandedBodyHeight(
        Pawn pawn,
        InvestitureSnapshot snapshot,
        DockRenderContext ctx
    );

    void DrawHeader(Rect rect, bool expanded);

    void DrawBody(
        Rect rect,
        Pawn pawn,
        InvestitureSnapshot snapshot,
        DockRenderContext ctx
    );
}
