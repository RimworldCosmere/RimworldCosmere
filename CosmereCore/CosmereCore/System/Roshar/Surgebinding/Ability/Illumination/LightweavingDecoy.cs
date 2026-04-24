using System;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Profile;
using DecoyHediff = Cosmere.System.Roshar.Surgebinding.Hediff.Illumination.LightweavingDecoy;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Illumination;

public class LightweavingDecoy : SurgebindingAbility {
    private static readonly List<Pawn> ActiveDecoys = [];

    public LightweavingDecoy(Pawn pawn) : base(pawn) { }
    public LightweavingDecoy(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    private int decoyCount => Math.Clamp(gene.currentIdeal, 1, 4);

    protected override void OnEnable() {
        base.OnEnable();
        SpawnDecoys();
    }

    protected override void OnDisable() {
        DestroyAllDecoys();
        base.OnDisable();
    }

    private void SpawnDecoys() {
        if (!pawn.Spawned || pawn.Map == null) return;

        int count = decoyCount;
        for (int i = 0; i < count; i++) {
            IntVec3 spawnCell = CellFinder.RandomSpawnCellForPawnNear(pawn.Position, pawn.Map, 3);
            if (!spawnCell.IsValid) continue;

            Pawn decoy = SpawnDecoyPawn(spawnCell, pawn.Map);
            if (decoy != null) {
                ActiveDecoys.Add(decoy);
                FleckMaker.Static(spawnCell, pawn.Map, FleckDefOf.PsycastAreaEffect);
            }
        }
    }

    private void DestroyAllDecoys() {
        for (int i = ActiveDecoys.Count - 1; i >= 0; i--) {
            Pawn decoy = ActiveDecoys[i];
            DecoyHediff? hediff = decoy.health?.hediffSet?.GetFirstHediffOfDef(DecoyHediff.Def) as DecoyHediff;
            if (hediff?.caster != pawn) continue;

            if (decoy.Spawned) {
                FleckMaker.Static(decoy.Position, decoy.Map, FleckDefOf.PsycastAreaEffect);
                decoy.DeSpawn();
            }

            if (!decoy.Destroyed) {
                decoy.Discard(true);
            }

            ActiveDecoys.RemoveAt(i);
        }
    }

    public static void RemoveDecoy(Pawn decoy) {
        ActiveDecoys.Remove(decoy);
    }

    private Pawn SpawnDecoyPawn(IntVec3 cell, Map map) {
        Pawn decoy = PawnGenerator.GeneratePawn(
            new PawnGenerationRequest(
                pawn.kindDef,
                Faction.OfPlayer,
                forceGenerateNewPawn: true
            )
        );

        SanitizeDecoy(decoy);
        CopyAppearance(pawn, decoy);

        decoy.Name = new NameSingle(pawn.Name?.ToStringShort + " (Decoy)");

        GenSpawn.Spawn(decoy, cell, map);

        DecoyHediff hediff = (DecoyHediff)HediffMaker.MakeHediff(DecoyHediff.Def, decoy);
        hediff.caster = pawn;
        hediff.Severity = 1f;
        decoy.health.AddHediff(hediff);

        return decoy;
    }

    private static void SanitizeDecoy(Pawn decoy) {
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
            List<Trait> allTraits = [..decoy.story.traits.allTraits];
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
            List<DirectPawnRelation> rels = [..decoy.relations.DirectRelations];
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
            List<Apparel> targetWorn = [..target.apparel.WornApparel];
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

    [HarmonyPatch(typeof(MemoryUtility), nameof(MemoryUtility.ClearAllMapsAndWorld))]
    public static class LightweavingDecoyStateClearer {
        [HarmonyPostfix]
        public static void Postfix() {
            ActiveDecoys.Clear();
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PreApplyDamage))]
    public static class VanishOnDamage {
        [HarmonyPrefix]
        public static bool Prefix(Pawn __instance, ref DamageInfo dinfo, out bool absorbed) {
            absorbed = false;
            if (!DecoyHediff.IsDecoy(__instance)) return true;

            absorbed = true;
            if (__instance.Spawned) {
                FleckMaker.Static(__instance.Position, __instance.Map, FleckDefOf.PsycastAreaEffect);
                __instance.DeSpawn();
            }

            RemoveDecoy(__instance);
            if (!__instance.Destroyed) {
                __instance.Discard(true);
            }

            return false;
        }
    }
}