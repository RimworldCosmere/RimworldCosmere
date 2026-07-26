using System;
using System.Text;
using Cosmere.Core.Ability;
using Cosmere.Core.Comp.Hediff;
using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Gene;
using Verse;

namespace Cosmere.Core.Hediff;

public interface IHediff<TGene> where TGene : Invested {
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

public abstract class AbstractHediff<TGene> : HediffWithComps, IHediff<TGene> where TGene : Invested {
    protected IAbility<TGene, IHediff<TGene>> ability = null!;
    private TGene geneInt = null!;
    protected AbstractHediff() { }

    protected AbstractHediff(HediffDef hediffDef, Pawn pawn, IAbility<TGene, IHediff<TGene>> ability) {
        def = hediffDef;
        this.pawn = pawn;
        this.ability = ability;
        Gene = ability.Gene;
    }

    public override string LabelBase =>
        base.LabelBase + (SourceAbilities.Count > 1 ? $" ({SourceAbilities.Count} sources)" : "");

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

        List<Pawn> sourcePawns = null!;
        Scribe_References.Look(ref geneInt, "gene");
        Scribe_References.Look(ref ability, "ability");

        if (Scribe.mode == LoadSaveMode.Saving) {
            HashSet<Pawn> seen = [];
            sourcePawns = [];
            foreach (IAbility<TGene, IHediff<TGene>> source in SourceAbilities) {
                if (source is RimWorld.Ability abilityRef && seen.Add(abilityRef.pawn)) {
                    sourcePawns.Add(abilityRef.pawn);
                }
            }
        }

        Scribe_Collections.Look(ref sourcePawns, "sourcePawns", LookMode.Reference);

        if (Scribe.mode != LoadSaveMode.LoadingVars) {
            return;
        }

        SourceAbilities.Clear();

        if (sourcePawns == null) {
            return;
        }

        foreach (Pawn? localPawn in sourcePawns) {
            foreach (RimWorld.Ability? ability in localPawn?.abilities?.abilities ?? []) {
                if (ability is not AbstractAbility<TGene, AbstractHediff<TGene>> aa) {
                    continue;
                }

                SourceAbilities.Add(aa);
                break;
            }
        }
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