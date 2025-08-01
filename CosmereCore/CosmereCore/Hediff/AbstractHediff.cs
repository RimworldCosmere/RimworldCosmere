using System;
using System.Text;
using Cosmere.Core.Ability;
using Cosmere.Core.Comp.Hediff;
using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Gene;
using Verse;

namespace Cosmere.Core.Hediff;

public interface IHediff<TGene> where TGene : Invested {
    public HashSet<IAbility<TGene, IHediff<TGene>>> sourceAbilities { get; }
    public float extraSeverity { get; set; }
    public float Severity { get; set; }
    public TGene gene { get; }
    public void AddSource(IAbility<TGene, IHediff<TGene>> sourceAbility);

    public void RemoveSource(IAbility<TGene, IHediff<TGene>> sourceAbility);
    public void PostMake();
    public event Action<AbstractHediff<TGene>, IAbility<TGene, IHediff<TGene>>>? OnSourceAdded;
    public event Action<AbstractHediff<TGene>, IAbility<TGene, IHediff<TGene>>>? OnSourceRemoved;
}

public abstract class AbstractHediff(HediffDef hediffDef, Pawn pawn, AbstractAbility ability)
    : AbstractHediff<Invested>(hediffDef, pawn, ability);

public abstract class AbstractHediff<TGene> : HediffWithComps, IHediff<TGene> where TGene : Invested {
    private TGene geneInt;
    public AbstractHediff() { }

    public AbstractHediff(HediffDef hediffDef, Pawn pawn, IAbility<TGene, IHediff<TGene>> ability) {
        def = hediffDef;
        this.pawn = pawn;
        gene = ability.gene;
    }

    public override string LabelBase =>
        base.LabelBase + (sourceAbilities.Count > 1 ? $" ({sourceAbilities.Count} sources)" : "");

    public SeverityCalculator<TGene>? severityCalculator => GetComp<SeverityCalculator<TGene>>();
    protected InvestitureHolder investiture => pawn.GetInvestiture();

    public TGene gene {
        get => geneInt;
        protected set => geneInt = value;
    }

    public float extraSeverity { get; set; } = 0f;
    public HashSet<IAbility<TGene, IHediff<TGene>>> sourceAbilities { get; } = [];

    public void AddSource(IAbility<TGene, IHediff<TGene>> sourceAbility) {
        sourceAbilities.Add(sourceAbility);
        OnSourceAdded?.Invoke(this, sourceAbility);
    }

    public void RemoveSource(IAbility<TGene, IHediff<TGene>> sourceAbility) {
        sourceAbilities.Remove(sourceAbility);
        OnSourceRemoved?.Invoke(this, sourceAbility);
    }

    public event Action<AbstractHediff<TGene>, IAbility<TGene, IHediff<TGene>>>? OnSourceAdded;
    public event Action<AbstractHediff<TGene>, IAbility<TGene, IHediff<TGene>>>? OnSourceRemoved;

    /**
     * @TODO This needs to support Nicrosil and Duralumin ACROSS the cosmere. Need to figure that out.
     */
    public override void TickInterval(int delta) {
        if (pawn.IsShieldedAgainstInvestiture() && !IsInvestitureShield()) {
            foreach (AbstractAbility? ability in sourceAbilities.Where(x => x is AbstractAbility)
                         .Cast<AbstractAbility>()
                         .ToList()) {
                ability.UpdateStatus(Active.Off);
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

        if (Scribe.mode == LoadSaveMode.Saving) {
            sourcePawns = sourceAbilities.Cast<RimWorld.Ability>().Select(a => a.pawn).Distinct().ToList();
        }

        Scribe_Collections.Look(ref sourcePawns, "sourcePawns", LookMode.Reference);

        if (Scribe.mode != LoadSaveMode.LoadingVars) {
            return;
        }

        sourceAbilities.Clear();

        if (sourcePawns == null) {
            return;
        }

        foreach (Pawn? localPawn in sourcePawns) {
            foreach (RimWorld.Ability? ability in localPawn?.abilities?.abilities ?? []) {
                if (ability is not AbstractAbility<TGene, AbstractHediff<TGene>> aa) {
                    continue;
                }

                // If this pawn has any Allomantic ability, we re-attach it
                sourceAbilities.Add(aa);
                // Optional: maybe only add one matching ability type, if you can correlate them
                break;
            }
        }
    }

    public override string DebugString() {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(base.DebugString());

        stringBuilder.AppendLine("Severity Sources:");
        foreach (AbstractAbility? ability in sourceAbilities.Cast<AbstractAbility>()) {
            stringBuilder.AppendLine(
                $"  {ability.pawn.NameShortColored} -> {ability.def.LabelCap}: {ability.GetStrength():0.0000}"
            );
        }

        return stringBuilder.ToString();
    }
}