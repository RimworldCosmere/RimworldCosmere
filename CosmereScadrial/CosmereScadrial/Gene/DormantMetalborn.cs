using System.Collections.Generic;
using Verse;

namespace CosmereScadrial.Gene;

public class DormantMetalborn : Verse.Gene {
    public List<GeneDef> genesToAdd = [];

    private bool snapped =>
        pawn?.needs?.mood?.thoughts?.memories.GetFirstMemoryOfDef(ThoughtDefOf.Cosmere_Scadrial_Snapped) != null;

    public override void Tick() {
        if (!snapped) {
            base.Tick();
            return;
        }

        foreach (GeneDef? gene in genesToAdd) {
            pawn.genes.AddGene(gene, true);
        }

        pawn.genes.RemoveGene(this);
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Collections.Look(ref genesToAdd, "genesToAdd", LookMode.Def);
    }
}