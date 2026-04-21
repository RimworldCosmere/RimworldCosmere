using Cosmere.Core.UI.Codex;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Radial;
using Cosmere.System.Scadrial.Extension;
using Cosmere.System.Scadrial.Feruchemy;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

public sealed class FeruchemyInvestitureProvider : IInvestitureProvider, ICodexContentProvider {
    private static readonly FeruchemyCodexContent codex = new();
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
                Label: f.metal.LabelCap,
                Current: totalValue,
                Max: totalMax,
                TargetValue: f.targetValue
            );

            cells.Add(new InvestitureCell(
                SubsystemId: f.metal.defName,
                Label: f.metal.LabelCap,
                Icon: f.metal.feruchemy?.icon,
                Bar: bar,
                IsActive: f.isTapping || f.isStoring,
                IsFlaring: f.isCompounding
            ));
        }

        if (cells.Count == 0) return null;

        return new InvestitureSnapshot(
            SystemId: SystemId,
            SystemLabel: "Feruchemy",
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
                    LeafId: "TAP",
                    Label: "Tap " + f.metal.LabelCap,
                    Icon: f.metal.feruchemy?.icon,
                    Kind: RadialActionKind.ToggleFeruchemyTap,
                    AbilityDef: null,
                    IsActive: f.isTapping,
                    IsFlaring: false,
                    IsSustained: f.isTapping,
                    IsLocked: totalMax <= 0f,
                    LockReason: totalMax <= 0f ? "No metalminds" : null,
                    ReserveFraction: reserveFraction,
                    HasInsufficientResources: f.isTapping && reserveFraction <= 0f,
                    CostHint: null,
                    CooldownTicksRemaining: 0
                ),
                new RadialLeaf(
                    LeafId: "STORE",
                    Label: "Store " + f.metal.LabelCap,
                    Icon: f.metal.feruchemy?.icon,
                    Kind: RadialActionKind.ToggleFeruchemyStore,
                    AbilityDef: null,
                    IsActive: f.isStoring,
                    IsFlaring: false,
                    IsSustained: f.isStoring,
                    IsLocked: totalMax <= 0f,
                    LockReason: totalMax <= 0f ? "No metalminds" : null,
                    ReserveFraction: reserveFraction,
                    HasInsufficientResources: false,
                    CostHint: null,
                    CooldownTicksRemaining: 0
                ),
                new RadialLeaf(
                    LeafId: "IDLE",
                    Label: "Idle",
                    Icon: null,
                    Kind: RadialActionKind.ResetFeruchemyIdle,
                    AbilityDef: null,
                    IsActive: !f.isTapping && !f.isStoring,
                    IsFlaring: false,
                    IsSustained: false,
                    IsLocked: false,
                    LockReason: null,
                    ReserveFraction: reserveFraction,
                    HasInsufficientResources: false,
                    CostHint: null,
                    CooldownTicksRemaining: 0
                ),
            ];

            AbilityDef? compoundDef = DefDatabase<AbilityDef>.GetNamedSilentFail(
                "Cosmere_Scadrial_Ability_Compound" + f.metal.defName
            );
            if (compoundDef != null && pawn.genes.HasAllomanticGeneForMetal(f.metal)) {
                RimWorld.Ability? compoundAbility = null;
                for (int j = 0; j < abilities.Count; j++) {
                    if (abilities[j].def == compoundDef) {
                        compoundAbility = abilities[j];
                        break;
                    }
                }
                bool canCast = compoundAbility != null && compoundAbility.CanCast;
                leaves.Add(new RadialLeaf(
                    LeafId: "COMPOUND",
                    Label: "Compound",
                    Icon: f.metal.feruchemy?.icon,
                    Kind: RadialActionKind.InvokeCompound,
                    AbilityDef: compoundDef,
                    IsActive: false,
                    IsFlaring: false,
                    IsSustained: false,
                    IsLocked: !canCast,
                    LockReason: canCast ? null : "Compound unavailable",
                    ReserveFraction: reserveFraction,
                    HasInsufficientResources: false,
                    CostHint: null,
                    CooldownTicksRemaining: compoundAbility != null ? compoundAbility.CooldownTicksRemaining : 0
                ));
            }

            subs.Add(new RadialSubsection(
                SubsectionId: f.metal.defName,
                Label: f.metal.LabelCap,
                Icon: f.metal.feruchemy?.icon,
                AccentColor: new Color(0.55f, 0.70f, 0.85f),
                Leaves: leaves
            ));
        }

        if (subs.Count == 0) return null;

        return new RadialSystem(
            SystemId: "Feruchemy",
            Label: "Feruchemy",
            Icon: null,
            Subsections: subs
        );
    }

    public bool HasProgression(Pawn pawn) => codex.HasProgression(pawn);
    public void DrawProgression(Pawn pawn, UnityEngine.Rect rect) => codex.DrawProgression(pawn, rect);
    public bool HasBonded(Pawn pawn) => codex.HasBonded(pawn);
    public void DrawBonded(Pawn pawn, UnityEngine.Rect rect) => codex.DrawBonded(pawn, rect);
    public bool HasMemories(Pawn pawn) => codex.HasMemories(pawn);
    public void DrawMemories(Pawn pawn, UnityEngine.Rect rect) => codex.DrawMemories(pawn, rect);
    public string? HeaderLabelFor(Pawn pawn) => codex.HeaderLabelFor(pawn);
}
