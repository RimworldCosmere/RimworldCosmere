using RimWorld;
using Verse;
using DecoyHediff = Cosmere.System.Roshar.Surgebinding.Hediff.Illumination.LightweavingDecoy;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Illumination;

public static class LightweavingDecoyFactory {
    public static Pawn Spawn(Pawn caster, IntVec3 cell, Map map) {
        Pawn decoy = PawnGenerator.GeneratePawn(
            new PawnGenerationRequest(
                caster.kindDef,
                Faction.OfPlayer,
                forceGenerateNewPawn: true
            )
        );

        Sanitize(decoy);
        CopyAppearance(caster, decoy);

        decoy.Name = new NameSingle(caster.Name?.ToStringShort + " (Decoy)");

        GenSpawn.Spawn(decoy, cell, map);

        DecoyHediff hediff = (DecoyHediff)HediffMaker.MakeHediff(DecoyHediff.Def, decoy);
        hediff.caster = caster;
        hediff.Severity = 1f;
        decoy.health.AddHediff(hediff);

        return decoy;
    }

    private static void Sanitize(Pawn decoy) {
        if (decoy.inventory != null) {
            decoy.inventory.DestroyAll();
        }

        if (decoy.equipment != null) {
            decoy.equipment.DestroyAllEquipment();
        }

        if (decoy.apparel != null) {
            decoy.apparel.DestroyAll();
        }

        if (decoy.story?.traits != null) {
            List<Trait> allTraits = [.. decoy.story.traits.allTraits];
            for (int i = 0; i < allTraits.Count; i++) {
                decoy.story.traits.RemoveTrait(allTraits[i]);
            }
        }

        if (decoy.skills != null) {
            List<SkillRecord> allSkills = decoy.skills.skills;
            for (int i = 0; i < allSkills.Count; i++) {
                allSkills[i].Level = 0;
                allSkills[i].passion = Passion.None;
            }
        }

        if (decoy.relations != null) {
            List<DirectPawnRelation> rels = [.. decoy.relations.DirectRelations];
            for (int i = 0; i < rels.Count; i++) {
                decoy.relations.RemoveDirectRelation(rels[i]);
            }
        }

        if (decoy.needs != null) {
            decoy.needs.AllNeeds.Clear();
        }

        if (decoy.workSettings != null) {
            decoy.workSettings.DisableAll();
        }

        decoy.playerSettings = new Pawn_PlayerSettings(decoy);
    }

    private static void CopyAppearance(Pawn source, Pawn target) {
        if (source.story == null || target.story == null) return;

        target.story.bodyType = source.story.bodyType;
        target.story.headType = source.story.headType;
        target.story.hairDef = source.story.hairDef;
        target.story.HairColor = source.story.HairColor;
        target.story.SkinColorBase = source.story.SkinColorBase;

        if (source.style != null && target.style != null) {
            target.style.beardDef = source.style.beardDef;
        }

        target.gender = source.gender;

        if (source.apparel != null && target.apparel != null) {
            List<Apparel> targetWorn = [.. target.apparel.WornApparel];
            for (int i = 0; i < targetWorn.Count; i++) {
                target.apparel.Remove(targetWorn[i]);
                targetWorn[i].Destroy();
            }

            List<Apparel> sourceWorn = source.apparel.WornApparel;
            for (int i = 0; i < sourceWorn.Count; i++) {
                Apparel copy = (Apparel)ThingMaker.MakeThing(sourceWorn[i].def, sourceWorn[i].Stuff);
                copy.SetColor(sourceWorn[i].DrawColor);
                target.apparel.Wear(copy, false);
            }
        }

        if (source.equipment?.Primary != null && target.equipment != null) {
            ThingWithComps weapon = (ThingWithComps)ThingMaker.MakeThing(
                source.equipment.Primary.def,
                source.equipment.Primary.Stuff
            );
            target.equipment.AddEquipment(weapon);
        }
    }
}
