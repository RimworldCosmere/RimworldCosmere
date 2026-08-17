using System;
using System.Text;
using Cosmere.Core.Ability;
using Cosmere.Core.Comp.Hediff;
using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Gene;
using Cosmere.Core.Util;
using Verse;

namespace Cosmere.Core.Hediff;

public interface IHediff<TGene>
    where TGene : Invested {
    public HashSet<IAbility<TGene, IHediff<TGene>>> SourceAbilities { get; }

    public float ExtraSeverity { get; set; }

    public float Severity { get; set; }

    public TGene Gene { get; }

    public void AddSource(IAbility<TGene, IHediff<TGene>> sourceAbility);

    public void RemoveSource(IAbility<TGene, IHediff<TGene>> sourceAbility);

    public void PostMake();

    public event Action<IHediff<TGene>, IAbility<TGene, IHediff<TGene>>>? OnSourceAdded;

    public event Action<IHediff<TGene>, IAbility<TGene, IHediff<TGene>>>? OnSourceRemoved;
}

public abstract class AbstractHediff(HediffDef hediffDef, Pawn pawn, AbstractAbility ability)
    : AbstractHediff<Invested>(hediffDef, pawn, ability);

public abstract class AbstractHediff<TGene> : HediffWithComps, IHediff<TGene>
    where TGene : Invested {
    protected IAbility<TGene, IHediff<TGene>> ability = null!;
    private TGene geneInt = null!;

    /// <summary>Which ability on each holder, so the right one comes back rather than the first.</summary>
    private List<string> sourceAbilityDefs = null!;

    /// <summary>Who was holding this when the game was saved.</summary>
    /// <remarks>
    ///     A field rather than a local, because pawn references stay null until the cross-reference
    ///     pass and the rebuild has to wait for PostLoadInit to read them.
    /// </remarks>
    private List<Pawn> sourcePawns = null!;

    protected AbstractHediff() { }

    protected AbstractHediff(HediffDef hediffDef, Pawn pawn, IAbility<TGene, IHediff<TGene>> ability) {
        def = hediffDef;
        this.pawn = pawn;
        this.ability = ability;
        Gene = ability.Gene;
    }

    public override string LabelBase =>
        base.LabelBase + (SourceAbilities.Count > 1 ? $" ({SourceAbilities.Count} sources)" : string.Empty);

    public SeverityCalculator<TGene>? severityCalculator => GetComp<SeverityCalculator<TGene>>();

    protected InvestitureHolder? investiture => pawn.GetInvestiture();

    public TGene Gene {
        get => geneInt;
        protected set => geneInt = value;
    }

    public float ExtraSeverity { get; set; } = 0f;

    public HashSet<IAbility<TGene, IHediff<TGene>>> SourceAbilities { get; } = [];

    public void AddSource(IAbility<TGene, IHediff<TGene>> sourceAbility) {
        SourceAbilities.Add(sourceAbility);
        OnSourceAdded?.Invoke(this, sourceAbility);
    }

    public void RemoveSource(IAbility<TGene, IHediff<TGene>> sourceAbility) {
        SourceAbilities.Remove(sourceAbility);
        OnSourceRemoved?.Invoke(this, sourceAbility);
    }

    public event Action<IHediff<TGene>, IAbility<TGene, IHediff<TGene>>>? OnSourceAdded;

    public event Action<IHediff<TGene>, IAbility<TGene, IHediff<TGene>>>? OnSourceRemoved;

    public override void TickInterval(int delta) {
        if (pawn.IsShieldedAgainstInvestiture() && !IsInvestitureShield()) {
            IAbility<TGene, IHediff<TGene>>[] snapshot = [.. SourceAbilities];
            for (int i = 0; i < snapshot.Length; i++) {
                if (snapshot[i] is AbstractAbility abilityToDisable) {
                    abilityToDisable.UpdateStatus(Active.Off);
                }
            }
        }

        if (pawn.IsHashIntervalTick(GenTicks.TickRareInterval, delta)) {
            severityCalculator?.RecalculateSeverity();
        }

        base.TickInterval(delta);
    }

    protected virtual bool IsInvestitureShield() {
        return false;
    }

    public override void ExposeData() {
        base.ExposeData();

        Scribe_References.Look(ref geneInt, "gene");
        Scribe_References.Look(ref ability, "ability");

        if (Scribe.mode == LoadSaveMode.Saving) {
            sourcePawns = [];
            sourceAbilityDefs = [];
            foreach (IAbility<TGene, IHediff<TGene>> source in SourceAbilities) {
                if (source is not RimWorld.Ability sourceAbility) continue;

                sourcePawns.Add(sourceAbility.pawn);
                sourceAbilityDefs.Add(sourceAbility.def.defName);
            }
        }

        Scribe_Collections.Look(ref sourcePawns, "sourcePawns", LookMode.Reference);
        Scribe_Collections.Look(ref sourceAbilityDefs, "sourceAbilityDefs", LookMode.Value);

        if (Scribe.mode != LoadSaveMode.PostLoadInit) {
            return;
        }

        RestoreSources();
    }

    /// <summary>
    ///     Puts back the abilities that were holding this hediff when the game was saved.
    /// </summary>
    /// <remarks>
    ///     Runs in PostLoadInit because the saved pawns are cross-references, and those are still
    ///     null through LoadingVars - which is where this used to run, walking a list of nulls and
    ///     restoring nothing. Each holder is then matched by the ability's own def instead of by
    ///     taking whatever sat first in their roster, which on a Mistborn is an arbitrary metal.
    /// </remarks>
    private void RestoreSources() {
        SourceAbilities.Clear();

        if (sourcePawns == null || sourceAbilityDefs == null) {
            return;
        }

        int count = Math.Min(sourcePawns.Count, sourceAbilityDefs.Count);
        List<string> rosterDefs = [];
        for (int i = 0; i < count; i++) {
            List<RimWorld.Ability> roster = sourcePawns[i]?.abilities?.abilities ?? [];

            rosterDefs.Clear();
            for (int j = 0; j < roster.Count; j++) {
                rosterDefs.Add(roster[j].def.defName);
            }

            int match = SourceAbilityMatch.IndexOf(rosterDefs, sourceAbilityDefs[i]);
            if (match < 0) continue;
            if (roster[match] is not AbstractAbility<TGene, AbstractHediff<TGene>> restored) continue;

            SourceAbilities.Add(restored);
        }

        sourcePawns = null!;
        sourceAbilityDefs = null!;
    }

    public override string DebugString() {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(base.DebugString());

        stringBuilder.AppendLine("Severity Sources:");
        foreach (IAbility<TGene, IHediff<TGene>> source in SourceAbilities) {
            if (source is not AbstractAbility sourceAbility) continue;
            stringBuilder.AppendLine(
                $"  {sourceAbility.pawn.NameShortColored} -> {sourceAbility.def.LabelCap}: {sourceAbility.GetStrength():0.0000}"
            );
        }

        return stringBuilder.ToString();
    }
}
