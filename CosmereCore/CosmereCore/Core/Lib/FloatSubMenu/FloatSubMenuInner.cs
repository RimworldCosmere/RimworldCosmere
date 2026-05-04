using UnityEngine;
using Verse;

namespace Cosmere.Core.Lib.FloatSubMenu;

internal class FloatSubMenuInner : FloatMenu {
    public readonly Vector2 mouseOffset;
    public readonly FloatSubMenu parent;

    public FloatSubMenuInner(FloatSubMenu parent, List<FloatMenuOption> options, Vector2 mouseOffset, bool vanish)
        : base(options) {
        this.mouseOffset = mouseOffset;
        this.parent = parent;
        onlyOneOfTypeAllowed = false;
        vanishIfMouseDistant = vanish;
        parent.UpdateFilter(this);
    }

    public override void DoWindowContents(Rect rect) {
        parent.UpdateFilter(this);
        base.DoWindowContents(rect);
    }

    protected override void SetInitialSizeAndPosition() {
        Vector2 pos = Verse.UI.MousePositionOnUIInverted + mouseOffset;
        Vector2 size = InitialSize;
        float x = Mathf.Min(pos.x, Verse.UI.screenWidth - size.x);
        float y = Mathf.Min(pos.y, Verse.UI.screenHeight - size.y);
        windowRect = new Rect(x, y, size.x, size.y);
    }

    public override void PreOptionChosen(FloatMenuOption opt) {
        parent.subMenuOptionChosen = true;
        base.PreOptionChosen(opt);
    }

    public override void PreClose() {
        foreach (FloatSubMenu sub in options.OfType<FloatSubMenu>()) {
            sub.CloseSubMenu();
        }

        OpenMenuSet.Close(this);
    }
}
