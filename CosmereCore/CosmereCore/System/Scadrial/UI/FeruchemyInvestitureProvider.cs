using Cosmere.Core.UI.Codex;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Radial;
using Cosmere.System.Scadrial.Feruchemy;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

public sealed class FeruchemyInvestitureProvider : IInvestitureProvider, ICodexContentProvider {
    private static readonly FeruchemyCodexContent codex = new FeruchemyCodexContent();

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

    public string SystemId => "Feruchemy";

    public bool IsInvested(Pawn pawn) {
        if (pawn.genes == null) return false;
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is Feruchemist f && !f.Overridden) return true;
        }

        return false;
    }

    public InvestitureSnapshot? Snapshot(Pawn pawn) {
        if (pawn.genes == null) return null;

        List<InvestitureCell> cells = [];
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is not Feruchemist f || f.Overridden) continue;

            List<IMetalmindSource> mms = f.metalminds;
            float totalMax = 0f;
            float totalValue = 0f;
            for (int j = 0; j < mms.Count; j++) {
                totalMax += mms[j].maxAmount;
                totalValue += mms[j].storedAmount;
            }

            ResourceBar bar = new ResourceBar(
                f.metal.LabelCap,
                totalValue,
                totalMax,
                f.targetValue
            );

            cells.Add(
                new InvestitureCell(
                    f.metal.defName,
                    f.metal.LabelCap,
                    f.metal.feruchemy?.icon,
                    bar,
                    f.isTapping || f.isStoring,
                    f.isCompounding
                )
            );
        }

        if (cells.Count == 0) return null;

        return new InvestitureSnapshot(
            SystemId,
            "Feruchemy",
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
            if (all[i] is not Feruchemist f || f.Overridden) continue;

            List<IMetalmindSource> mms = f.metalminds;
            float totalMax = 0f;
            float totalValue = 0f;
            for (int j = 0; j < mms.Count; j++) {
                totalMax += mms[j].maxAmount;
                totalValue += mms[j].storedAmount;
            }

            float reserveFraction = totalMax > 0f ? totalValue / totalMax : 0f;

            List<RadialLeaf> leaves = [
                new RadialLeaf(
                    "TAP",
                    "Tap " + f.metal.LabelCap,
                    f.metal.feruchemy?.icon,
                    RadialActionKind.ToggleFeruchemyTap,
                    null,
                    f.isTapping,
                    false,
                    f.isTapping,
                    totalMax <= 0f,
                    totalMax <= 0f ? "No metalminds" : null,
                    reserveFraction,
                    f.isTapping && reserveFraction <= 0f,
                    null,
                    0
                ),
                new RadialLeaf(
                    "STORE",
                    "Store " + f.metal.LabelCap,
                    f.metal.feruchemy?.icon,
                    RadialActionKind.ToggleFeruchemyStore,
                    null,
                    f.isStoring,
                    false,
                    f.isStoring,
                    totalMax <= 0f,
                    totalMax <= 0f ? "No metalminds" : null,
                    reserveFraction,
                    false,
                    null,
                    0
                ),
                new RadialLeaf(
                    "IDLE",
                    "Idle",
                    null,
                    RadialActionKind.ResetFeruchemyIdle,
                    null,
                    !f.isTapping && !f.isStoring,
                    false,
                    false,
                    false,
                    null,
                    reserveFraction,
                    false,
                    null,
                    0
                ),
            ];

            AbilityDef? compoundDef = DefDatabase<AbilityDef>.GetNamedSilentFail(
                "Cosmere_Scadrial_Ability_Compound" + f.metal.defName
            );
            if (compoundDef != null && pawn.genes.HasAllomanticGeneForMetal(f.metal)) {
                Ability? compoundAbility = null;
                for (int j = 0; j < abilities.Count; j++) {
                    if (abilities[j].def == compoundDef) {
                        compoundAbility = abilities[j];
                        break;
                    }
                }

                bool canCast = compoundAbility != null && compoundAbility.CanCast;
                leaves.Add(
                    new RadialLeaf(
                        "COMPOUND",
                        "Compound",
                        f.metal.feruchemy?.icon,
                        RadialActionKind.InvokeCompound,
                        compoundDef,
                        false,
                        false,
                        false,
                        !canCast,
                        canCast ? null : "Compound unavailable",
                        reserveFraction,
                        false,
                        null,
                        compoundAbility != null ? compoundAbility.CooldownTicksRemaining : 0
                    )
                );
            }

            subs.Add(
                new RadialSubsection(
                    f.metal.defName,
                    f.metal.LabelCap,
                    f.metal.feruchemy?.icon,
                    new Color(0.55f, 0.70f, 0.85f),
                    leaves
                )
            );
        }

        if (subs.Count == 0) return null;

        return new RadialSystem(
            "Feruchemy",
            "Feruchemy",
            null,
            subs
        );
    }
}