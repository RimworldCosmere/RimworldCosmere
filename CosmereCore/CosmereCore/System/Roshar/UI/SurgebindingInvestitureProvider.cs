using Cosmere.Core.UI.Codex;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Radial;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Surgebinding.Ability;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.UI;

public sealed class SurgebindingInvestitureProvider : CodexInvestitureProviderBase<SurgebindingCodexContent> {
    public override string SystemId => "Surgebinding";

    public override bool IsInvested(Pawn pawn) {
        if (pawn.genes == null) return false;
        return pawn.genes.GetFirstGeneOfType<Surgebinder>() != null;
    }

    public override InvestitureSnapshot? Snapshot(Pawn pawn) {
        Surgebinder? s = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (s == null) return null;

        ResourceBar bar = new ResourceBar(
            "Stormlight",
            s.Value,
            s.Max,
            s.targetValue
        );

        return new InvestitureSnapshot(
            SystemId,
            "Surgebinding",
            bar,
            []
        );
    }

    public override RadialSystem? SnapshotRadial(Pawn pawn) {
        if (pawn.genes == null || pawn.abilities == null) return null;

        List<RadialSubsection> subs = [];
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        List<Ability> abilities = pawn.abilities.AllAbilitiesForReading;

        for (int i = 0; i < all.Count; i++) {
            if (all[i] is not Surgebinder s || s.Overridden) continue;
            RadiantOrderDef order = s.radiantOrderDef;

            List<RadialLeaf> leaves = [];
            HashSet<AbilityDef> seen = [];
            foreach (AbilityDef def in order.GetAbilities(s.CurrentIdeal)) {
                if (IsExcludedFromRadial(def)) continue;
                if (!seen.Add(def)) continue;

                int minIdeal = def is SurgebindingAbilityDef sd ? sd.GetMinIdealForOrder(order.defName) : 0;
                bool locked = s.CurrentIdeal < minIdeal;

                Ability? ability = null;
                for (int j = 0; j < abilities.Count; j++) {
                    if (abilities[j].def == def) {
                        ability = abilities[j];
                        break;
                    }
                }

                leaves.Add(BuildRadialLeaf(def, ability, locked, minIdeal, s));
            }

            for (int j = 0; j < abilities.Count; j++) {
                if (abilities[j] is not SurgebindingAbility sa) continue;
                AbilityDef def = sa.def;
                if (IsExcludedFromRadial(def)) continue;
                if (!seen.Add(def)) continue;

                leaves.Add(BuildRadialLeaf(def, sa, false, 0, s));
            }

            if (leaves.Count == 0) continue;

            subs.Add(
                new RadialSubsection(
                    order.defName,
                    order.LabelCap,
                    order.icon,
                    order.color,
                    leaves
                )
            );
        }

        if (subs.Count == 0) return null;

        return new RadialSystem(
            "Surgebinding",
            "Stormlight",
            null,
            subs
        );
    }

    private static bool IsExcludedFromRadial(AbilityDef def) {
        return def is SurgebindingAbilityDef sd && !sd.showInRadial;
    }

    private static RadialLeaf BuildRadialLeaf(AbilityDef def, Ability? ability, bool locked, int minIdeal, Surgebinder s) {
        bool isActive = ability is SurgebindingAbility sa && sa.status.IsActive;
        bool canCast = !locked && ability != null && ability.CanCast;
        int cooldown = ability?.CooldownTicksRemaining ?? 0;

        return new RadialLeaf(
            LeafId: def.defName,
            Label: def.LabelCap,
            Icon: def.uiIcon,
            Kind: RadialActionKind.CastAbility,
            AbilityDef: def,
            IsActive: isActive,
            IsFlaring: false,
            IsSustained: isActive && def.cooldownTicksRange.max == 0,
            IsLocked: locked,
            LockReason: locked ? $"Requires Ideal {minIdeal}" : null,
            ReserveFraction: s.Max > 0f ? s.Value / s.Max : 0f,
            HasInsufficientResources: !canCast && !locked,
            CostHint: null,
            CooldownTicksRemaining: cooldown
        );
    }
}
