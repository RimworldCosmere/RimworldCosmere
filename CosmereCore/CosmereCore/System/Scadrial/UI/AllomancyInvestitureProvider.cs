using Cosmere.Core.Ability;
using Cosmere.Core.UI.Codex;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Radial;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

public sealed class AllomancyInvestitureProvider : CodexInvestitureProviderBase<AllomancyCodexContent> {
    public const string Id = "Allomancy";

    public override string SystemId => Id;

    public override bool IsInvested(Pawn pawn) {
        if (pawn.genes == null) return false;
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is Allomancer a && !a.Overridden) return true;
        }

        return false;
    }

    // def order, not the pawn's grant order - grant order puts an arbitrary steel ability first.
    private static List<AllomancyAbility> AbilitiesFor(List<Ability> abilities, MetallicArtsMetalDef metal) {
        List<AllomancyAbility> matched = [];
        for (int i = 0; i < abilities.Count; i++) {
            if (abilities[i] is AllomancyAbility a && a.Metal == metal) matched.Add(a);
        }

        List<AllomanticAbilityDef> order = DefDatabase<AllomanticAbilityDef>.AllDefsListForReading;
        matched.Sort((x, y) => order.IndexOf(x.Def).CompareTo(order.IndexOf(y.Def)));
        return matched;
    }

    private static InvestitureAbility ToCellAbility(AllomancyAbility a) {
        return new InvestitureAbility(
            a.Def.defName,
            a.Def.LabelCap,
            a.Def,
            a.Def.targetRequired,
            a.Def.maxPower > 1,
            a.AtLeastBurning,
            a.status.power > 1
        );
    }

    public override InvestitureSnapshot? Snapshot(Pawn pawn) {
        if (pawn.genes == null) return null;

        List<InvestitureCell> cells = [];
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        List<Ability> dockAbilities = pawn.abilities?.AllAbilitiesForReading ?? [];
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is not Allomancer a || a.Overridden) continue;

            List<AllomancyAbility> matched = AbilitiesFor(dockAbilities, a.Metal);
            List<InvestitureAbility> cellAbilities = [];
            bool anyActive = false;
            bool anyFlaring = false;
            for (int j = 0; j < matched.Count; j++) {
                InvestitureAbility entry = ToCellAbility(matched[j]);
                cellAbilities.Add(entry);
                anyActive |= entry.IsActive;
                anyFlaring |= entry.IsFlaring;
            }

            ResourceBar bar = new ResourceBar(
                a.Metal.LabelCap,
                a.Value,
                a.Max,
                a.targetValue
            );

            cells.Add(
                new InvestitureCell(
                    a.Metal.defName,
                    a.Metal.LabelCap,
                    a.Metal.allomancy?.invertedIcon,
                    bar,
                    matched.Count > 0 ? anyActive : a.Burning,
                    anyFlaring,
                    cellAbilities
                )
            );
        }

        if (cells.Count == 0) return null;

        return new InvestitureSnapshot(
            SystemId,
            "Allomancy",
            null,
            cells
        );
    }

    public override RadialSystem? SnapshotRadial(Pawn pawn) {
        if (pawn.genes == null) return null;

        List<RadialSubsection> subs = [];
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        List<Ability> abilities = pawn.abilities?.AllAbilitiesForReading ?? [];

        for (int i = 0; i < all.Count; i++) {
            if (all[i] is not Allomancer a || a.Overridden) continue;

            List<AllomancyAbility> matched = AbilitiesFor(abilities, a.Metal);
            if (matched.Count == 0) continue;

            float reserveFraction = a.Max > 0f ? a.Value / a.Max : 0f;

            subs.Add(
                new RadialSubsection(
                    a.Metal.defName,
                    a.Metal.LabelCap,
                    a.Metal.allomancy?.invertedIcon,
                    new Color(0.75f, 0.65f, 0.45f),
                    BuildLeaves(matched, a, reserveFraction)
                )
            );
        }

        if (subs.Count == 0) return null;

        return RadialSystem.ForSystem("Allomancy", subs);
    }

    /// <summary>One leaf per ability the metal carries, all reading the same reserve.</summary>
    private static List<RadialLeaf> BuildLeaves(
        List<AllomancyAbility> matched, Allomancer gene, float reserveFraction
    ) {
        List<RadialLeaf> leaves = [];
        for (int j = 0; j < matched.Count; j++) {
            AllomancyAbility ability = matched[j];
            bool targeted = ability.Def.targetRequired;

            leaves.Add(
                new RadialLeaf(
                    LeafId: ability.Def.defName,
                    Label: ability.AtLeastBurning
                        ? "CC_Radial_Action_StopBurning".Translate(ability.Def.LabelCap.Named("METAL"))
                        : ability.Def.LabelCap,
                    Icon: gene.Metal.allomancy?.invertedIcon,
                    Kind: targeted ? RadialActionKind.CastAbility : RadialActionKind.StartAllomancyBurn,
                    AbilityDef: ability.Def,
                    IsActive: ability.AtLeastBurning,
                    IsFlaring: ability.status.power > 1,
                    CanFlare: ability.Def.maxPower > 1,
                    IsSustained: ability.Def.toggleable && ability.status.IsActive,
                    IsLocked: false,
                    LockReason: null,
                    ReserveFraction: reserveFraction,
                    HasInsufficientResources: reserveFraction <= 0f,
                    CostHint:
                    $"{ability.GetDesiredBurnRateForStatus(Status.PowerOne) * GenTicks.TicksPerRealSecond:F2}/s",
                    Description: ability.Def.description,
                    CooldownTicksRemaining: 0
                )
            );
        }

        return leaves;
    }
}
