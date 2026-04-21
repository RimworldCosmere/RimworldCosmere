using Cosmere.Core.UI.Codex;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Radial;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Surgebinding.Ability;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.UI;

public sealed class SurgebindingInvestitureProvider : IInvestitureProvider, ICodexContentProvider {
    private static readonly SurgebindingCodexContent codex = new();
    public string SystemId => "Surgebinding";

    public bool IsInvested(Pawn pawn) {
        if (pawn.genes == null) return false;
        return pawn.genes.GetFirstGeneOfType<Surgebinder>() != null;
    }

    public InvestitureSnapshot? Snapshot(Pawn pawn) {
        Surgebinder? s = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (s == null) return null;

        ResourceBar bar = new ResourceBar(
            Label: "Stormlight",
            Current: s.Value,
            Max: s.Max,
            TargetValue: s.targetValue
        );

        return new InvestitureSnapshot(
            SystemId: SystemId,
            SystemLabel: "Surgebinding",
            PrimaryBar: bar,
            Cells: [],
            Subsections: [],
            FlatAbilities: []
        );
    }

    public RadialSystem? SnapshotRadial(Pawn pawn) {
        if (pawn.genes == null || pawn.abilities == null) return null;

        List<RadialSubsection> subs = [];
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        List<RimWorld.Ability> abilities = pawn.abilities.AllAbilitiesForReading;

        for (int i = 0; i < all.Count; i++) {
            if (all[i] is not Surgebinder s || s.Overridden) continue;
            RadiantOrderDef order = s.radiantOrderDef;

            List<RadialLeaf> leaves = [];
            foreach (AbilityDef def in order.GetAbilities(s.currentIdeal)) {
                if (IsExcludedFromRadial(def)) continue;

                int minIdeal = def is SurgebindingAbilityDef sd ? sd.GetMinIdealForOrder(order.defName) : 0;
                bool locked = s.currentIdeal < minIdeal;

                RimWorld.Ability? ability = null;
                for (int j = 0; j < abilities.Count; j++) {
                    if (abilities[j].def == def) {
                        ability = abilities[j];
                        break;
                    }
                }

                bool isActive = ability is SurgebindingAbility sa && sa.status.isActive;
                bool canCast = !locked && ability != null && ability.CanCast;
                int cooldown = ability?.CooldownTicksRemaining ?? 0;

                leaves.Add(new RadialLeaf(
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
                ));
            }

            if (leaves.Count == 0) continue;

            subs.Add(new RadialSubsection(
                SubsectionId: order.defName,
                Label: order.LabelCap,
                Icon: order.icon,
                AccentColor: order.color,
                Leaves: leaves
            ));
        }

        if (subs.Count == 0) return null;

        return new RadialSystem(
            SystemId: "Surgebinding",
            Label: "Stormlight",
            Icon: null,
            Subsections: subs
        );
    }

    private static bool IsExcludedFromRadial(AbilityDef def) {
        return def.defName == "Cosmere_Roshar_Ability_ToggleShardblade"
            || def.defName == "Cosmere_Roshar_Ability_ToggleShardplate";
    }

    public bool HasProgression(Pawn pawn) => codex.HasProgression(pawn);
    public void DrawProgression(Pawn pawn, UnityEngine.Rect rect) => codex.DrawProgression(pawn, rect);
    public bool HasBonded(Pawn pawn) => codex.HasBonded(pawn);
    public void DrawBonded(Pawn pawn, UnityEngine.Rect rect) => codex.DrawBonded(pawn, rect);
    public bool HasMemories(Pawn pawn) => codex.HasMemories(pawn);
    public void DrawMemories(Pawn pawn, UnityEngine.Rect rect) => codex.DrawMemories(pawn, rect);
}
