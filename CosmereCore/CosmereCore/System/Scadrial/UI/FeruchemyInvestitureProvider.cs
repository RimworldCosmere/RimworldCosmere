using Cosmere.Core.UI.Codex;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Radial;
using Cosmere.System.Scadrial.Feruchemy;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

public sealed class FeruchemyInvestitureProvider : CodexInvestitureProviderBase<FeruchemyCodexContent> {
    public const string Id = "Feruchemy";
    public override string SystemId => Id;

    public override bool IsInvested(Pawn pawn) {
        if (pawn.genes == null) return false;
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is Feruchemist f && !f.Overridden) return true;
        }

        return false;
    }

    public override InvestitureSnapshot? Snapshot(Pawn pawn) {
        if (pawn.genes == null) return null;

        List<InvestitureCell> cells = [];
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is not Feruchemist f || f.Overridden) continue;

            List<IMetalmindSource> mms = f.metalminds;
            float totalMax = 0f;
            float totalValue = 0f;
            for (int j = 0; j < mms.Count; j++) {
                totalMax += mms[j].MaxAmount;
                totalValue += mms[j].StoredAmount;
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
            cells
        );
    }

    public override RadialSystem? SnapshotRadial(Pawn pawn) {
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
                totalMax += mms[j].MaxAmount;
                totalValue += mms[j].StoredAmount;
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
                    Description: null,
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
                    Description: null,
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
                    Description: null,
                    CooldownTicksRemaining: 0
                ),
            ];

            AbilityDef? compoundDef = f.metal.GetCompoundAbility();
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
                        Description: compoundDef.description,
                        CooldownTicksRemaining: compoundAbility != null ? compoundAbility.CooldownTicksRemaining : 0
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
