using System;
using System.Collections.Generic;
using Cosmere.Core.Settings;
using Verse;

namespace Cosmere.Core.Comp.Thing;

public class DormantConnection : ThingComp {
    private Dictionary<GeneDef, (int, Func<Pawn, bool>)> hiddenGenes = [];
    public bool hasDormantConnections => hiddenGenes.Count > 0;
    private Pawn pawn => (Pawn)parent;

    public override void CompTickInterval(int delta) {
        base.CompTickInterval(delta);

        foreach ((GeneDef? gene, (int interval, Func<Pawn, bool> callback)) in hiddenGenes) {
            if (!pawn.IsHashIntervalTick(interval, delta)) continue;
            if (!callback(pawn)) continue;

            pawn.genes.AddGene(gene, true);
            hiddenGenes.Remove(gene);
        }
    }

    public void AddHiddenGene(GeneDef geneDef, (int, Func<Pawn, bool>) intervalAndFunc) {
        hiddenGenes.Add(geneDef, intervalAndFunc);
    }

    public void AddHiddenGene(GeneDef geneDef, Func<Pawn, bool> func) {
        hiddenGenes.Add(geneDef, (1, func));
    }

    public override void PostExposeData() {
        base.PostExposeData();

        Scribe_Collections.Look(ref hiddenGenes, "hiddenGenes", LookMode.Def);
    }

    public override string CompInspectStringExtra() {
        if (!Framework.Mod.GetModSettings<CoreModSettings>().showDormantConnection || hiddenGenes.Count == 0) {
            return base.CompInspectStringExtra();
        }

        return "CC_PawnHasDormantConnection".Translate(pawn.Named("PAWN"));
    }
}