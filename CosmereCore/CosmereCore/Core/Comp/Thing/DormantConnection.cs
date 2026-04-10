using System;
using Cosmere.Core.Settings;
using Verse;

namespace Cosmere.Core.Comp.Thing;

public struct HiddenGeneData : IExposable {
    public GeneDef gene;
    public int interval;

    public void ExposeData() {
        Scribe_Defs.Look(ref gene, "gene");
        Scribe_Values.Look(ref interval, "interval");
    }
}

public class DormantConnection : ThingComp {
    private Dictionary<GeneDef, int> hiddenGenes = [];
    public bool hasDormantConnections => hiddenGenes.Count > 0;
    private Pawn pawn => (Pawn)parent;

    public override void CompTickInterval(int delta) {
        base.CompTickInterval(delta);

        foreach ((GeneDef? gene, int interval) in hiddenGenes.ToList()) {
            if (!pawn.IsHashIntervalTick(interval, delta)) continue;
            if (!GetGeneCallback(gene)(pawn, gene)) continue;

            pawn.genes.AddGene(gene, true);
            hiddenGenes.Remove(gene);
        }
    }

    private static Func<Pawn, GeneDef, bool> GetGeneCallback(GeneDef gene) {
        if (!gene.HasModExtension<DefModExtension.DormantConnection>()) return (_, _) => false;

        return gene.GetModExtension<DefModExtension.DormantConnection>()!.Handler!.callback;
    }

    public void AddHiddenGene(GeneDef geneDef, int interval = 1) {
        hiddenGenes.Add(geneDef, interval);
    }

    public override void PostExposeData() {
        base.PostExposeData();

        Scribe_Collections.Look(ref hiddenGenes, "hiddenGenes", LookMode.Def);
    }

    public override string CompInspectStringExtra() {
        if (!Mod.GetModSettings<CoreModSettings>().showDormantConnection || hiddenGenes.Count == 0) {
            return base.CompInspectStringExtra();
        }

        return "CC_PawnHasDormantConnection".Translate(pawn.Named("PAWN"));
    }
}