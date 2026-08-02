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

    // Def order rather than the pawn's ability order, which is grant order and puts an
    // arbitrary steel ability first.
    private static List<AllomancyAbility> AbilitiesFor(IReadOnlyList<Ability> abilities, MetallicArtsMetalDef metal) {
        List<AllomancyAbility> matched = [];
        for (int i = 0; i < abilities.Count; i++) {
            if (abilities[i] is AllomancyAbility a && a.metal == metal) matched.Add(a);
        }

        List<AllomanticAbilityDef> order = DefDatabase<AllomanticAbilityDef>.AllDefsListForReading;
        matched.Sort((x, y) => order.IndexOf(x.def).CompareTo(order.IndexOf(y.def)));
        return matched;
    }

    private static InvestitureAbility ToCellAbility(AllomancyAbility a) {
        return new InvestitureAbility(
            a.def.defName,
            a.def.LabelCap,
            a.def,
            a.def.targetRequired,
            a.def.maxPower > 1,
            a.atLeastBurning,
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

            List<AllomancyAbility> matched = AbilitiesFor(dockAbilities, a.metal);
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
                a.metal.LabelCap,
                a.Value,
                a.Max,
                a.targetValue
            );

            cells.Add(
                new InvestitureCell(
                    a.metal.defName,
                    a.metal.LabelCap,
                    a.metal.allomancy?.invertedIcon,
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

            List<AllomancyAbility> matched = AbilitiesFor(abilities, a.metal);
            if (matched.Count == 0) continue;

            float reserveFraction = a.Max > 0f ? a.Value / a.Max : 0f;
            List<RadialLeaf> leaves = [];
            for (int j = 0; j < matched.Count; j++) {
                AllomancyAbility ability = matched[j];
                bool targeted = ability.def.targetRequired;

                leaves.Add(
                    new RadialLeaf(
                        LeafId: ability.def.defName,
                        Label: ability.atLeastBurning
                            ? "CC_Radial_Action_StopBurning".Translate(ability.def.LabelCap.Named("METAL"))
                            : ability.def.LabelCap,
                        Icon: a.metal.allomancy?.invertedIcon,
                        Kind: targeted ? RadialActionKind.CastAbility : RadialActionKind.StartAllomancyBurn,
                        AbilityDef: ability.def,
                        IsActive: ability.atLeastBurning,
                        IsFlaring: ability.status.power > 1,
                        IsSustained: ability.def.toggleable && ability.status.IsActive,
                        IsLocked: false,
                        LockReason: null,
                        ReserveFraction: reserveFraction,
                        HasInsufficientResources: reserveFraction <= 0f,
                        CostHint:
                        $"{ability.GetDesiredBurnRateForStatus(Status.PowerOne) * GenTicks.TicksPerRealSecond:F2}/s",
                        Description: ability.def.description,
                        CooldownTicksRemaining: 0
                    )
                );
            }

            subs.Add(
                new RadialSubsection(
                    a.metal.defName,
                    a.metal.LabelCap,
                    a.metal.allomancy?.invertedIcon,
                    new Color(0.75f, 0.65f, 0.45f),
                    leaves
                )
            );
        }

        if (subs.Count == 0) return null;

        return RadialSystem.ForSystem("Allomancy", subs);
    }
}
