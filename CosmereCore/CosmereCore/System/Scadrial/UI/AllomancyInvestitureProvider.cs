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

public sealed class AllomancyInvestitureProvider : IInvestitureProvider, ICodexContentProvider {
    private static readonly AllomancyCodexContent codex = new();
    public string SystemId => "Allomancy";

    public bool IsInvested(Pawn pawn) {
        if (pawn.genes == null) return false;
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is Allomancer a && !a.Overridden) return true;
        }
        return false;
    }

    public InvestitureSnapshot? Snapshot(Pawn pawn) {
        if (pawn.genes == null) return null;

        List<InvestitureCell> cells = [];
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is not Allomancer a || a.Overridden) continue;

            ResourceBar bar = new ResourceBar(
                Label: a.metal.LabelCap,
                Current: a.Value,
                Max: a.Max,
                TargetValue: a.targetValue
            );

            cells.Add(new InvestitureCell(
                SubsystemId: a.metal.defName,
                Label: a.metal.LabelCap,
                Icon: a.metal.allomancy?.invertedIcon,
                Bar: bar,
                IsActive: a.Burning,
                IsFlaring: false
            ));
        }

        if (cells.Count == 0) return null;

        return new InvestitureSnapshot(
            SystemId: SystemId,
            SystemLabel: "Allomancy",
            PrimaryBar: null,
            Cells: cells,
            Subsections: [],
            FlatAbilities: []
        );
    }

    public RadialSystem? SnapshotRadial(Pawn pawn) {
        if (pawn.genes == null) return null;

        List<RadialSubsection> subs = [];
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        List<RimWorld.Ability> abilities = pawn.abilities?.AllAbilitiesForReading ?? [];

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
                Label: "Burn " + a.metal.LabelCap,
                Icon: a.metal.allomancy?.invertedIcon,
                Kind: RadialActionKind.StartAllomancyBurn,
                AbilityDef: matched.def,
                IsActive: matched.atLeastBurning,
                IsFlaring: matched.status.power > 1,
                IsSustained: matched.def.toggleable && matched.status.isActive,
                IsLocked: false,
                LockReason: null,
                ReserveFraction: reserveFraction,
                HasInsufficientResources: reserveFraction <= 0f,
                CostHint: $"{matched.GetDesiredBurnRateForStatus(Status.PowerOne) * GenTicks.TicksPerRealSecond:F2}/s",
                CooldownTicksRemaining: 0
            );

            subs.Add(new RadialSubsection(
                SubsectionId: a.metal.defName,
                Label: a.metal.LabelCap,
                Icon: a.metal.allomancy?.invertedIcon,
                AccentColor: new Color(0.75f, 0.65f, 0.45f),
                Leaves: [leaf]
            ));
        }

        if (subs.Count == 0) return null;

        return new RadialSystem(
            SystemId: "Allomancy",
            Label: "Allomancy",
            Icon: null,
            Subsections: subs
        );
    }

    public bool HasProgression(Pawn pawn) => codex.HasProgression(pawn);
    public void DrawProgression(Pawn pawn, UnityEngine.Rect rect) => codex.DrawProgression(pawn, rect);
    public bool ShowsBondsSubtab => codex.ShowsBondsSubtab;
    public bool HasBonds(Pawn pawn) => codex.HasBonds(pawn);
    public void DrawBonds(Pawn pawn, UnityEngine.Rect rect, CodexState state) => codex.DrawBonds(pawn, rect, state);
    public bool HasMemories(Pawn pawn) => codex.HasMemories(pawn);
    public void DrawMemories(Pawn pawn, UnityEngine.Rect rect) => codex.DrawMemories(pawn, rect);
    public bool OwnsAbility(RimWorld.Ability ability) => codex.OwnsAbility(ability);
    public string? HeaderLabelFor(Pawn pawn) => codex.HeaderLabelFor(pawn);
}
