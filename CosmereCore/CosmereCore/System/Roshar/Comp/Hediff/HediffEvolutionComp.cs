using Cosmere.System.Roshar.Def;
using RimWorld;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar.Comp.Hediff;

public class HediffEvolutionCompProperties : HediffCompProperties {
    public HediffEvolutionCompProperties() {
        compClass = typeof(HediffEvolutionComp);
    }
}

public class HediffEvolutionComp : HediffComp {
    private int evolutionTick = -1;
    private bool evolved;
    private NightwatcherCurseDef? curseDef;

    public void Initialize(NightwatcherCurseDef curse) {
        curseDef = curse;
        if (curse.cultivationEvolutionDays <= 0) return;
        evolutionTick = GenTicks.TicksGame + curse.cultivationEvolutionDays * GenDate.TicksPerDay;
        Logger.Info($"HediffEvolutionComp: scheduled evolution in {curse.cultivationEvolutionDays} days for {Pawn?.NameShortColored}");
    }

    public override void CompPostTickInterval(ref float severityAdjustment, int delta) {
        if (evolved || evolutionTick < 0) return;
        if (GenTicks.TicksGame < evolutionTick) return;
        if (!Pawn.IsHashIntervalTick(GenTicks.TickRareInterval, delta)) return;
        Evolve();
    }

    private void Evolve() {
        evolved = true;
        if (curseDef == null) return;
        Verse.Pawn pawn = parent.pawn;

        if (curseDef.evolutionHediff != null)
            pawn.health.AddHediff(HediffMaker.MakeHediff(curseDef.evolutionHediff, pawn));

        if (curseDef.evolutionGrantTrait != null && !pawn.story.traits.HasTrait(curseDef.evolutionGrantTrait))
            pawn.story.traits.GainTrait(new Trait(curseDef.evolutionGrantTrait));

        Find.LetterStack.ReceiveLetter(
            "Cosmere_Roshar_Nightwatcher_Evolution_Title".Translate(pawn.Named("PAWN")),
            "Cosmere_Roshar_Nightwatcher_Evolution_Desc".Translate(
                pawn.Named("PAWN"), curseDef.label.Named("CURSE")),
            RimWorld.LetterDefOf.NeutralEvent,
            pawn
        );

        Logger.Info($"HediffEvolutionComp: {pawn.NameShortColored} curse '{curseDef.defName}' has evolved");
    }

    public override void CompExposeData() {
        base.CompExposeData();
        Scribe_Values.Look(ref evolutionTick, "evolutionTick", -1);
        Scribe_Values.Look(ref evolved, "evolved");
        Scribe_Defs.Look(ref curseDef, "curseDef");
    }
}
