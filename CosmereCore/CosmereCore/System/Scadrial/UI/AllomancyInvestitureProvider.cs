using Cosmere.Core.Ability;
using Cosmere.Core.UI.Codex;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Radial;
using Cosmere.System.Scadrial.Allomancy.Ability;
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

    public override InvestitureSnapshot? Snapshot(Pawn pawn) {
        if (pawn.genes == null) return null;

        List<InvestitureCell> cells = [];
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        List<Ability> dockAbilities = pawn.abilities?.AllAbilitiesForReading ?? [];
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is not Allomancer a || a.Overridden) continue;

            AllomancyAbility? burn = null;
            for (int j = 0; j < dockAbilities.Count; j++) {
                if (dockAbilities[j] is AllomancyAbility aa && aa.metal == a.metal) {
                    burn = aa;
                    break;
                }
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
                    burn?.atLeastBurning ?? a.Burning,
                    burn != null && burn.status.power > 1
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

            AllomancyAbility? matched = null;
            for (int j = 0; j < abilities.Count; j++) {
                if (abilities[j] is AllomancyAbility aa && aa.metal == a.metal) {
                    matched = aa;
                    break;
                }
            }

            if (matched == null) continue;

            float reserveFraction = a.Max > 0f ? a.Value / a.Max : 0f;

            RadialLeaf leaf = new RadialLeaf(
                LeafId: "BURN",
                Label: matched.atLeastBurning
                    ? "CC_Radial_Action_StopBurning".Translate(a.metal.LabelCap.Named("METAL"))
                    : "CC_Radial_Action_Burn".Translate(a.metal.LabelCap.Named("METAL")),
                Icon: a.metal.allomancy?.invertedIcon,
                Kind: RadialActionKind.StartAllomancyBurn,
                AbilityDef: matched.def,
                IsActive: matched.atLeastBurning,
                IsFlaring: matched.status.power > 1,
                IsSustained: matched.def.toggleable && matched.status.IsActive,
                IsLocked: false,
                LockReason: null,
                ReserveFraction: reserveFraction,
                HasInsufficientResources: reserveFraction <= 0f,
                CostHint: $"{matched.GetDesiredBurnRateForStatus(Status.PowerOne) * GenTicks.TicksPerRealSecond:F2}/s",
                Description: matched.def.description,
                CooldownTicksRemaining: 0
            );

            subs.Add(
                new RadialSubsection(
                    a.metal.defName,
                    a.metal.LabelCap,
                    a.metal.allomancy?.invertedIcon,
                    new Color(0.75f, 0.65f, 0.45f),
                    [leaf]
                )
            );
        }

        if (subs.Count == 0) return null;

        return RadialSystem.ForSystem("Allomancy", subs);
    }
}
