using RimWorld;
using Verse;

namespace Cosmere.Core.Util;

/// <summary>
///     Builds a visual copy of a pawn without running PawnGenerator, so no traits, skills, gear,
///     ideo, backstories or relations are rolled and nothing outside the copy is touched.
/// </summary>
public static class IllusoryPawnUtility {
    public static Pawn Create(Pawn source, PawnKindDef kind, Faction? faction = null, bool copyGear = true) {
        Pawn illusion = (Pawn)ThingMaker.MakeThing(kind.race);
        illusion.kindDef = kind;
        illusion.SetFactionDirect(faction);
        PawnComponentsUtility.CreateInitialComponents(illusion);

        illusion.gender = source.gender;
        illusion.ageTracker.AgeChronologicalTicks = source.ageTracker.AgeChronologicalTicks;
        illusion.ageTracker.AgeBiologicalTicks = source.ageTracker.AgeBiologicalTicks;

        CopyAppearance(source, illusion);

        if (copyGear) {
            CopyGear(source, illusion);
        }

        illusion.needs?.SetInitialLevels();

        // DisableAll on its own leaves priorities null, which RimWorld logs an error about.
        if (illusion.workSettings != null) {
            illusion.workSettings.EnableAndInitialize();
            illusion.workSettings.DisableAll();
        }

        illusion.playerSettings = new Pawn_PlayerSettings(illusion);

        return illusion;
    }

    private static void CopyAppearance(Pawn source, Pawn target) {
        if (source.story == null || target.story == null) return;

        target.story.bodyType = source.story.bodyType;
        target.story.headType = source.story.headType;
        target.story.hairDef = source.story.hairDef;
        target.story.HairColor = source.story.HairColor;
        target.story.SkinColorBase = source.story.SkinColorBase;

        // The setter dereferences the value, so a null backstory has to stay unset.
        if (source.story.Childhood != null) {
            target.story.Childhood = source.story.Childhood;
        }

        if (source.story.Adulthood != null) {
            target.story.Adulthood = source.story.Adulthood;
        }

        if (source.style != null && target.style != null) {
            target.style.beardDef = source.style.beardDef;
        }
    }

    private static void CopyGear(Pawn source, Pawn target) {
        if (source.apparel != null && target.apparel != null) {
            List<Apparel> worn = source.apparel.WornApparel;
            for (int i = 0; i < worn.Count; i++) {
                Apparel copy = (Apparel)ThingMaker.MakeThing(worn[i].def, worn[i].Stuff);
                copy.SetColor(worn[i].DrawColor);
                target.apparel.Wear(copy, false);
            }
        }

        if (source.equipment?.Primary == null || target.equipment == null) return;

        ThingWithComps weapon = (ThingWithComps)ThingMaker.MakeThing(
            source.equipment.Primary.def,
            source.equipment.Primary.Stuff
        );
        target.equipment.AddEquipment(weapon);
    }
}
