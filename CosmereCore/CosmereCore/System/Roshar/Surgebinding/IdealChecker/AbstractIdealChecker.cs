using Cosmere.Core.Need;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Settings;
using RimWorld;
using UnityEngine;
using Verse;
using TraitRequirement = Verse.TraitRequirement;

namespace Cosmere.System.Roshar.Surgebinding.IdealChecker;

public abstract class AbstractIdealChecker(RadiantOrderDef def) {
    protected readonly RadiantOrderDef def = def;

    public abstract bool IsSatisfied(Pawn pawn, Surgebinder surgebinder, int nextLevel);

    public virtual string? GetRequirementsText(int idealIndex) {
        return null;
    }

    protected float ApplyDifficulty(float threshold) {
        RosharModSettings settings = Core.Mod.GetModSettings<RosharModSettings>();
        return threshold * settings.progressionDifficulty;
    }

    public virtual bool Satisfy(Pawn pawn, Surgebinder surgebinder, int nextLevel) {
        Pawn_NeedsTracker needs = pawn.needs;

        Thought_Memory oathThought = ThoughtMaker.MakeThought(ThoughtDefOf.Cosmere_Roshar_Thought_OathSpoken, 0);
        needs?.mood?.thoughts?.memories?.TryGainMemory(oathThought);

        Investiture? investiture = needs?.TryGetNeed(Core.NeedDefOf.Cosmere_Investiture) as Investiture;
        if (investiture != null) {
            investiture.CurLevel = investiture.MaxLevel;
        }

        ApplyWitnessThoughts(pawn);
        SpawnOathBurst(pawn);
        RemoveBondStrain(pawn);

        return true;
    }

    private void SpawnOathBurst(Pawn pawn) {
        if (pawn.Map == null) return;

        Mote mote = MoteMaker.MakeStaticMote(pawn.DrawPos, pawn.Map, ThingDefOf.Cosmere_Roshar_Mote_OathBurst, 7f);
        if (mote != null) {
            mote.instanceColor = new Color(def.color.r, def.color.g, def.color.b, 0.8f);
        }
    }

    private void RemoveBondStrain(Pawn pawn) {
        Verse.Hediff? stainedBond =
            pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.Cosmere_Roshar_Hediff_StrainedBond);
        if (stainedBond != null) {
            pawn.health!.RemoveHediff(stainedBond);
        }
    }

    public string? GetIncompatibleTraitName(Pawn pawn) {
        if (def.incompatibleTraits == null) return null;

        Pawn_StoryTracker story = pawn.story;
        if (story == null) return null;

        List<TraitRequirement> incompatible = def.incompatibleTraits;
        for (int i = 0; i < incompatible.Count; i++) {
            if (incompatible[i].HasTrait(pawn))
                return incompatible[i].def?.label ?? incompatible[i].def?.defName ?? "unknown";
        }

        return null;
    }

    public bool HasIncompatibleTrait(Pawn pawn, int nextLevel) {
        if (nextLevel < 2) return false;
        if (def.incompatibleTraits == null) return false;

        Pawn_StoryTracker story = pawn.story;
        if (story == null) return false;

        List<TraitRequirement> incompatible = def.incompatibleTraits;
        for (int i = 0; i < incompatible.Count; i++) {
            if (incompatible[i].HasTrait(pawn)) return true;
        }

        return false;
    }

    protected float GetTraitMultiplier(Pawn pawn) {
        if (def.favorableTraits == null) return 1f;

        Pawn_StoryTracker story = pawn.story;
        if (story == null) return 1f;

        List<TraitRequirement> favorable = def.favorableTraits;
        for (int i = 0; i < favorable.Count; i++) {
            if (favorable[i].HasTrait(pawn)) return 1.25f;
        }

        return 1f;
    }

    private void ApplyWitnessThoughts(Pawn oathSpeaker) {
        Map map = oathSpeaker.Map;
        if (map == null) return;

        List<Pawn> mapPawns = map.mapPawns.FreeColonistsSpawned;
        for (int i = 0; i < mapPawns.Count; i++) {
            Pawn colonist = mapPawns[i];
            if (colonist == oathSpeaker) continue;
            if (colonist.needs?.mood?.thoughts?.memories == null) continue;

            bool isPsychopath = colonist.story?.traits?.HasTrait(RimWorld.TraitDefOf.Psychopath) ?? false;
            if (isPsychopath) continue;

            bool isJealous = colonist.story?.traits?.HasTrait(RimWorld.TraitDefOf.Jealous) ?? false;
            ThoughtDef thoughtDef = isJealous
                ? ThoughtDefOf.Cosmere_Roshar_Thought_WitnessedOathJealous
                : ThoughtDefOf.Cosmere_Roshar_Thought_WitnessedOathPositive;

            Thought_Memory witnessThought = ThoughtMaker.MakeThought(thoughtDef, 0);
            colonist.needs.mood.thoughts.memories.TryGainMemory(witnessThought);
        }
    }
}