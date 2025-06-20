using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereCore.Needs;

public class Investiture : Need {
    public const int MaxInvestiture = 1000000;

    public Investiture(Pawn pawn) : base(pawn) {
        threshPercents = new List<float> { 0.1f, 0.25f, 0.5f, 0.75f };
    }

    public override float MaxLevel => MaxInvestiture;

    public override bool ShowOnNeedList =>
        pawn != null && pawn.story.traits.HasTrait(TraitDef.Named("Cosmere_Invested"));

    public override void SetInitialLevel() {
        CurLevel = 0f; // start uninvested
    }

    public override void NeedInterval() {
        if (IsFrozen) return;

        // It should only fall in specific cases. I'll need to figure this out
        // e.g. 
        //   * Allomancers should slowly lose their investiture over the day
        //   * Feruchemists lose it at all (their abilities make them lose it)
        //   * Radiants should quickly lose their investiture over the day

        /*
         * Pseudo code
                if (HasGene("Allomancer"))
                {
                    CurLevel -= FallPerTick * 150f; // 150 ticks between calls
                    CurLevel = Mathf.Clamp01(CurLevel);
                }

                if (HasGene("Radiant"))
                {
                    CurLevel -= FallPerTick * 40f; // 40 ticks between calls
                    CurLevel = Mathf.Clamp01(CurLevel);
                }
         */
    }

    public override string GetTipString() {
        return
            $"Represents this pawn’s connection to the Spiritual Realm. Increased by ingesting Allomantic metals.\n\nCurrent Level: {Mathf.RoundToInt(CurLevel * 100)}%";
    }
}