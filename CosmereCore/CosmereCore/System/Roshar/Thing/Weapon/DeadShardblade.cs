using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Def;
using Cosmere.Core.Shader.Properties;
using Cosmere.System.Roshar.Util;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Thing.Weapon;

public class DeadShardblade : ThingWithComps, IDynamicPalette {
    public override Color DrawColor {
        get {
            if (Stuff == null) return base.DrawColor;
            Color stuffColor = Stuff.stuffProps.color;
            return Color.Lerp(stuffColor, Color.gray, 0.5f);
        }
    }

    /// <summary>
    ///     A dead blade takes its colour from the gem it is made of. The secondary and glow tones come
    ///     from the matching <see cref="GemDef" /> when there is one, so a sapphire blade reads the same
    ///     way a sapphire does; everything is dulled toward gray to match <see cref="DrawColor" />,
    ///     because a dead blade has lost its lustre. The last three entries are the hilt, matching the
    ///     living blade's.
    /// </summary>
    public List<LUTPaletteMaterial> GetMaterials() {
        Color baseColor = DrawColor;
        GemDef? gem = GemForStuff();

        Color colorOne = baseColor;
        Color colorTwo = gem?.colorTwo is { } second ? Dull(second) : baseColor;
        Color colorThree = gem?.glowColor is { } glow ? Dull(glow) : colorTwo;

        return [
            new LUTPaletteMaterial { color = colorOne, metallic = .3f, smoothness = .7f },
            new LUTPaletteMaterial { color = colorTwo, metallic = .8f, smoothness = .85f },
            new LUTPaletteMaterial { color = colorThree, smoothness = 1 },
            new LUTPaletteMaterial
                { color = def.GetColorForStuff(Core.ThingDefOf.Gold), metallic = 1, smoothness = 1 },
            new LUTPaletteMaterial
                { color = def.GetColorForStuff(RimWorld.ThingDefOf.WoodLog) },
            new LUTPaletteMaterial
                { color = def.GetColorForStuff(Core.ThingDefOf.Steel), metallic = .5f, smoothness = 1 },
        ];
    }

    private GemDef? GemForStuff() {
        if (Stuff == null) return null;

        List<GemDef> gems = DefDatabase<GemDef>.AllDefsListForReading;
        for (int i = 0; i < gems.Count; i++) {
            if (gems[i].Item == Stuff) return gems[i];
        }

        return null;
    }

    private static Color Dull(Color color) {
        return Color.Lerp(color, Color.gray, 0.5f);
    }

    public override void Notify_Equipped(Verse.Pawn pawn) {
        base.Notify_Equipped(pawn);

        if (!CasteUtility.IsDarkeyes(pawn)) return;

        CasteUtility.DarkeyesToLighteyes(pawn, "Cosmere_Roshar_Gene_Dahn_Low");

        Find.LetterStack.ReceiveLetter(
            "Eyes of the Blade",
            $"The Shardblade bonds to {pawn.NameShortColored}'s soul. Over the following hours, {pawn.gender.GetPronoun()} eyes lighten. A darkeyes no longer - at least by the law of the blade.",
            RimWorld.LetterDefOf.PositiveEvent,
            pawn
        );
    }

    protected override void DrawAt(Vector3 drawLoc, bool flip = false) {
        CutoutAdvanced? comp = GetComp<CutoutAdvanced>();
        if (comp == null) {
            base.DrawAt(drawLoc, flip);
            return;
        }

        Graphic graphic = Graphic;
        Material material = graphic.MatAt(Rotation, this);

        comp.UpdateMaterialPropertyBlock(CutoutAdvanced.MPB, graphic, material);

        Mesh mesh = graphic.MeshAt(Rotation);
        Graphics.DrawMesh(mesh, drawLoc, Quaternion.identity, material, 0, null, 0, CutoutAdvanced.MPB);
    }
}
