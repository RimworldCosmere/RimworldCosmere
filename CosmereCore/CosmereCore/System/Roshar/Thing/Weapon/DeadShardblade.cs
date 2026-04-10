using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Thing.Weapon;

public class DeadShardblade : ThingWithComps {
    public override void Notify_Equipped(Verse.Pawn pawn) {
        base.Notify_Equipped(pawn);

        if (!Utility.CasteUtility.IsDarkeyes(pawn)) return;

        Utility.CasteUtility.DarkeyesToLighteyes(pawn, "Cosmere_Roshar_Gene_Dahn_Low");

        Find.LetterStack.ReceiveLetter(
            "Eyes of the Blade",
            $"The Shardblade bonds to {pawn.NameShortColored}'s soul. Over the following hours, {pawn.gender.GetPronoun()} eyes lighten. A darkeyes no longer - at least by the law of the blade.",
            RimWorld.LetterDefOf.PositiveEvent,
            pawn
        );
    }

    public override Color DrawColor {
        get {
            if (Stuff == null) return base.DrawColor;
            Color stuffColor = Stuff.stuffProps.color;
            return Color.Lerp(stuffColor, Color.gray, 0.5f);
        }
    }

    protected override void DrawAt(Vector3 drawLoc, bool flip = false) {
        Cosmere.Core.Comp.Thing.CutoutAdvanced? comp = GetComp<Cosmere.Core.Comp.Thing.CutoutAdvanced>();
        if (comp == null) {
            base.DrawAt(drawLoc, flip);
            return;
        }

        Verse.Graphic graphic = Graphic;
        Material material = graphic.MatAt(Rotation, this);

        comp.UpdateMaterialPropertyBlock(Cosmere.Core.Comp.Thing.CutoutAdvanced.MPB, graphic, material);

        Mesh mesh = graphic.MeshAt(Rotation);
        Graphics.DrawMesh(mesh, drawLoc, Quaternion.identity, material, 0, null, 0, Cosmere.Core.Comp.Thing.CutoutAdvanced.MPB);
    }
}
