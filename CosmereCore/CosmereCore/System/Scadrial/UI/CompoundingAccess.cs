using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Extension;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.UI;

// Compounding is never granted to a pawn, so its ability is built on demand for a
// twinborn holding both genes for the metal. Shared because both arts ask about
// it: Allomancy offers the action, Feruchemy decides whether the compounded pool
// is worth showing at all.
public static class CompoundingAccess {
    public const int SkillFloor = 10;

    private const string ResearchDefName = "Cosmere_Scadrial_Compounding";

    private static readonly Dictionary<string, AllomancyAbility?> cache =
        new Dictionary<string, AllomancyAbility?>();

    private static int cachedPawnId = -1;

    // Whether the pawn has any business seeing compounding controls. Deliberately
    // looser than Gate - the controls appear once either half is earned, and then
    // say what the other half is still missing.
    public static bool Discovered(Pawn pawn) {
        if (Research is { IsFinished: true }) return true;

        return SkillLevel(pawn, SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower) >= SkillFloor &&
               SkillLevel(pawn, SkillDefOf.Cosmere_Scadrial_Skill_FeruchemicPower) >= SkillFloor;
    }

    // Whether compounding this metal can actually start right now, and if not, why.
    public static AcceptanceReport Gate(Pawn pawn, Feruchemist gene, AllomancyAbility ability) {
        if (Research is { IsFinished: false }) {
            return "CC_Dock_Feruchemy_CompoundNoResearch".Translate(Research.LabelCap.Named("RESEARCH"));
        }

        if (SkillLevel(pawn, SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower) < SkillFloor ||
            SkillLevel(pawn, SkillDefOf.Cosmere_Scadrial_Skill_FeruchemicPower) < SkillFloor) {
            return "CC_Dock_Feruchemy_CompoundLowSkill".Translate(SkillFloor.Named("LEVEL"));
        }

        if (!gene.canStoreCompounded) return "CC_Dock_Feruchemy_CompoundNoImplant".Translate();

        return ability.CanCast;
    }

    public static AllomancyAbility? AbilityFor(Pawn pawn, string metalDefName) {
        if (cachedPawnId != pawn.thingIDNumber) {
            cache.Clear();
            cachedPawnId = pawn.thingIDNumber;
        }

        if (cache.TryGetValue(metalDefName, out AllomancyAbility? cached)) return cached;

        AllomancyAbility? made = null;
        MetallicArtsMetalDef? metal = DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(metalDefName);
        if (metal != null && pawn.IsMisting(metal)) {
            AllomanticAbilityDef? def = metal.GetCompoundAbility();
            if (def != null) made = AbilityUtility.MakeAbility(def, pawn) as AllomancyAbility;
        }

        cache[metalDefName] = made;
        return made;
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
