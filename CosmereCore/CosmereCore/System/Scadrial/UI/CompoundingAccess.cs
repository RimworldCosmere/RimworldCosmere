using Cosmere.System.Scadrial.Extension;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.UI;

// Compounding is storing into a metalmind you can also burn, so the whole of it
// lives in the Feruchemy panel. This decides whether a pawn has earned the right
// to burn one, which is the allomantic half of the trick.
public static class CompoundingAccess {
    public const int SkillFloor = 10;

    private const string ResearchDefName = "Cosmere_Scadrial_Compounding";

    // Whether the pawn has any business seeing compounding controls. Deliberately
    // looser than Gate - the controls appear once either half is earned, and then
    // say what the other half is still missing.
    public static bool Discovered(Pawn pawn) {
        if (Research is { IsFinished: true }) return true;

        return SkillLevel(pawn, SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower) >= SkillFloor &&
               SkillLevel(pawn, SkillDefOf.Cosmere_Scadrial_Skill_FeruchemicPower) >= SkillFloor;
    }

    // Whether this pawn may burn this metal's metalmind, and if not, why.
    public static AcceptanceReport Gate(Pawn pawn, Feruchemist gene) {
        if (Research is { IsFinished: false }) {
            return "CC_Dock_Feruchemy_CompoundNoResearch".Translate(Research.LabelCap.Named("RESEARCH"));
        }

        if (SkillLevel(pawn, SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower) < SkillFloor ||
            SkillLevel(pawn, SkillDefOf.Cosmere_Scadrial_Skill_FeruchemicPower) < SkillFloor) {
            return "CC_Dock_Feruchemy_CompoundLowSkill".Translate(SkillFloor.Named("LEVEL"));
        }

        if (!pawn.genes.HasAllomanticGeneForMetal(gene.metal)) {
            return "CC_Dock_Feruchemy_CompoundNoAllomancy".Translate(gene.metal.label.Named("METAL"));
        }

        // Deliberately not gated on there being room right now. This only points the
        // dial at the compounded pool; the dial's own halves grey themselves when
        // there is nothing to give or take, and a button that vanishes because a
        // metalmind happens to be full reads as broken.
        return true;
    }

    public static Feruchemist? FeruchemistFor(Pawn pawn, string metalDefName) {
        if (pawn.genes == null) return null;

        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is Feruchemist f && f.metal.defName == metalDefName && !f.Overridden) return f;
        }

        return null;
    }

    private static ResearchProjectDef? Research =>
        DefDatabase<ResearchProjectDef>.GetNamedSilentFail(ResearchDefName);

    private static int SkillLevel(Pawn pawn, SkillDef skill) {
        return pawn.skills?.GetSkill(skill).Level ?? 0;
    }
}
