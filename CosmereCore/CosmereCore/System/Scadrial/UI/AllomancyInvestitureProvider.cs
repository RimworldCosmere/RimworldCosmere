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
    private static readonly AllomancyCodexContent codex = new AllomancyCodexContent();

    public bool HasProgression(Pawn pawn) {
        return codex.HasProgression(pawn);
    }

    public void DrawProgression(Pawn pawn, Rect rect, CodexState state) {
        codex.DrawProgression(pawn, rect, state);
    }

    public bool ShowsBondsSubtab => codex.ShowsBondsSubtab;

    public bool HasBonds(Pawn pawn) {
        return codex.HasBonds(pawn);
    }

    public void DrawBonds(Pawn pawn, Rect rect, CodexState state) {
        codex.DrawBonds(pawn, rect, state);
    }

    public bool HasMemories(Pawn pawn) {
        return codex.HasMemories(pawn);
    }

    public void DrawMemories(Pawn pawn, Rect rect, CodexState state) {
        codex.DrawMemories(pawn, rect, state);
    }

    public bool OwnsAbility(Ability ability) {
        return codex.OwnsAbility(ability);
    }

    public string? HeaderLabelFor(Pawn pawn) {
        return codex.HeaderLabelFor(pawn);
    }

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
                    a.Burning,
                    false
                )
            );
        }

        if (cells.Count == 0) return null;

        return new InvestitureSnapshot(
            SystemId,
            "Allomancy",
            null,
            cells,
            [],
            []
        );
    }

    public RadialSystem? SnapshotRadial(Pawn pawn) {
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
                "BURN",
                "Burn " + a.metal.LabelCap,
                a.metal.allomancy?.invertedIcon,
                RadialActionKind.StartAllomancyBurn,
                matched.def,
                matched.atLeastBurning,
                matched.status.power > 1,
                matched.def.toggleable && matched.status.isActive,
                false,
                null,
                reserveFraction,
                reserveFraction <= 0f,
                $"{matched.GetDesiredBurnRateForStatus(Status.PowerOne) * GenTicks.TicksPerRealSecond:F2}/s",
                0
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

        return new RadialSystem(
            "Allomancy",
            "Allomancy",
            null,
            subs
        );
    }
}