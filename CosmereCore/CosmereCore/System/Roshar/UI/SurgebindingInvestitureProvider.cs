using Cosmere.Core.UI.Codex;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Radial;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Surgebinding.Ability;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.UI;

public sealed class SurgebindingInvestitureProvider : IInvestitureProvider, ICodexContentProvider {
    private static readonly SurgebindingCodexContent codex = new SurgebindingCodexContent();

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

    public string SystemId => "Surgebinding";

    public bool IsInvested(Pawn pawn) {
        if (pawn.genes == null) return false;
        return pawn.genes.GetFirstGeneOfType<Surgebinder>() != null;
    }

    public InvestitureSnapshot? Snapshot(Pawn pawn) {
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
            [],
            [],
            []
        );
    }

    public RadialSystem? SnapshotRadial(Pawn pawn) {
        if (pawn.genes == null || pawn.abilities == null) return null;

        List<RadialSubsection> subs = [];
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        List<Ability> abilities = pawn.abilities.AllAbilitiesForReading;

        for (int i = 0; i < all.Count; i++) {
            if (all[i] is not Surgebinder s || s.Overridden) continue;
            RadiantOrderDef order = s.radiantOrderDef;

            List<RadialLeaf> leaves = [];
            HashSet<AbilityDef> seen = [];
            foreach (AbilityDef def in order.GetAbilities(s.currentIdeal)) {
                if (IsExcludedFromRadial(def)) continue;
                if (!seen.Add(def)) continue;

                int minIdeal = def is SurgebindingAbilityDef sd ? sd.GetMinIdealForOrder(order.defName) : 0;
                bool locked = s.currentIdeal < minIdeal;

                Ability? ability = null;
                for (int j = 0; j < abilities.Count; j++) {
                    if (abilities[j].def == def) {
                        ability = abilities[j];
                        break;
                    }
                }

                bool isActive = ability is SurgebindingAbility sa && sa.status.isActive;
                bool canCast = !locked && ability != null && ability.CanCast;
                int cooldown = ability?.CooldownTicksRemaining ?? 0;

                leaves.Add(
                    new RadialLeaf(
                        def.defName,
                        def.LabelCap,
                        def.uiIcon,
                        RadialActionKind.CastAbility,
                        def,
                        isActive,
                        false,
                        isActive && def.cooldownTicksRange.max == 0,
                        locked,
                        locked ? $"Requires Ideal {minIdeal}" : null,
                        s.Max > 0f ? s.Value / s.Max : 0f,
                        !canCast && !locked,
                        null,
                        cooldown
                    )
                );
            }

            for (int j = 0; j < abilities.Count; j++) {
                if (abilities[j] is not SurgebindingAbility sa) continue;
                AbilityDef def = sa.def;
                if (IsExcludedFromRadial(def)) continue;
                if (!seen.Add(def)) continue;

                bool isActive = sa.status.isActive;
                bool canCast = sa.CanCast;
                int cooldown = sa.CooldownTicksRemaining;

                leaves.Add(
                    new RadialLeaf(
                        def.defName,
                        def.LabelCap,
                        def.uiIcon,
                        RadialActionKind.CastAbility,
                        def,
                        isActive,
                        false,
                        isActive && def.cooldownTicksRange.max == 0,
                        false,
                        null,
                        s.Max > 0f ? s.Value / s.Max : 0f,
                        !canCast,
                        null,
                        cooldown
                    )
                );
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
        return def.defName == "Cosmere_Roshar_Ability_ToggleShardblade" ||
               def.defName == "Cosmere_Roshar_Ability_ToggleShardplate";
    }
}