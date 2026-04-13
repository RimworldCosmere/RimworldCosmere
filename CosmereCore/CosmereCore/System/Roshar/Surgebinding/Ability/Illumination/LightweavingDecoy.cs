using System;
using Cosmere.Core.Ability;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Profile;
using DecoyHediff = Cosmere.System.Roshar.Surgebinding.Hediff.Illumination.LightweavingDecoy;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Illumination;

public class LightweavingDecoy : SurgebindingAbility {
    private static readonly List<Verse.Pawn> ActiveDecoys = [];

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

            Verse.Pawn decoy = SpawnDecoyPawn(spawnCell, pawn.Map);
            if (decoy != null) {
                ActiveDecoys.Add(decoy);
                FleckMaker.Static(spawnCell, pawn.Map, FleckDefOf.PsycastAreaEffect);
            }
        }
    }

    private void DestroyAllDecoys() {
        for (int i = ActiveDecoys.Count - 1; i >= 0; i--) {
            Verse.Pawn decoy = ActiveDecoys[i];
            DecoyHediff? hediff = decoy.health?.hediffSet?.GetFirstHediffOfDef(DecoyHediff.Def) as DecoyHediff;
            if (hediff?.caster != pawn) continue;

            if (decoy.Spawned) {
                FleckMaker.Static(decoy.Position, decoy.Map, FleckDefOf.PsycastAreaEffect);
                decoy.DeSpawn(DestroyMode.Vanish);
            }

            if (!decoy.Destroyed) {
                decoy.Discard(true);
            }

            ActiveDecoys.RemoveAt(i);
        }
    }

    public static void RemoveDecoy(Verse.Pawn decoy) {
        ActiveDecoys.Remove(decoy);
    }

    private Verse.Pawn SpawnDecoyPawn(IntVec3 cell, Verse.Map map) {
        Verse.Pawn decoy = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
            pawn.kindDef,
            Faction.OfPlayer,
            PawnGenerationContext.NonPlayer,
            forceGenerateNewPawn: true
        ));

        CopyAppearance(pawn, decoy);

        decoy.Name = new NameSingle(pawn.Name?.ToStringShort + " (Decoy)");

        if (decoy.skills != null) {
            List<SkillRecord> allSkills = decoy.skills.skills;
            for (int i = 0; i < allSkills.Count; i++) {
                allSkills[i].Level = 0;
                allSkills[i].passion = Passion.None;
            }
        }

        GenSpawn.Spawn(decoy, cell, map);

        DecoyHediff hediff = (DecoyHediff)HediffMaker.MakeHediff(DecoyHediff.Def, decoy);
        hediff.caster = pawn;
        hediff.Severity = 1f;
        decoy.health.AddHediff(hediff);

        if (decoy.drafter != null) {
            decoy.drafter.Drafted = true;
        }

        return decoy;
    }

    private static void CopyAppearance(Verse.Pawn source, Verse.Pawn target) {
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
}
