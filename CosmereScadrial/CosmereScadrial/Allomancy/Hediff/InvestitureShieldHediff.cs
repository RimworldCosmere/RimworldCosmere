using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Allomancy.Ability;
using CosmereScadrial.Def;
using CosmereScadrial.Extension;
using CosmereScadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Allomancy.Hediff;

public class InvestitureShieldHediff(HediffDef hediffDef, Pawn pawn, AbstractAbility ability)
    : AllomanticHediff(hediffDef, pawn, ability) {
    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        MetallicArtsMetalDef? metal = sourceAbilities.FirstOrDefault()?.metal;
        if (metal == null) return;

        List<Allomancer> genes = pawn.genes.GetAllomanticGenes();
        genes.RemoveWhere(x => Mathf.Approximately(x.Value, 0f));
        if (metal == MetallicArtsMetalDefOf.Aluminum) {
            genes.RemoveWhere(x => x.metal == MetallicArtsMetalDefOf.Aluminum);
        }

        if (genes.Count == 0) return;

        foreach (Allomancer gene in genes) {
            gene.WipeReserve();
        }

        Messages.Message(
            GetMessage(metal, genes.Select(g => g.metal).ToList()),
            MessageTypeDefOf.NeutralEvent,
            false
        );
    }

    // @todo Add translations
    private string GetMessage(MetallicArtsMetalDef metal, List<MetallicArtsMetalDef> metalsToWipe) {
        string metalsToWipeString = FormatDefList(metalsToWipe);
        if (metal.Equals(MetallicArtsMetalDefOf.Aluminum)) {
            return
                $"{pawn.NameFullColored} is burning {metal.LabelCap} and has wiped his reserves of {metalsToWipeString}.";
        }

        if (metal.Equals(MetallicArtsMetalDefOf.Chromium)) {
            return
                $"{pawn.NameFullColored} is the target of a lurcher, and is losing all of their reserves of {metalsToWipeString}.";
        }

        return "";
    }

    public static string FormatDefList(List<MetallicArtsMetalDef> defs) {
        List<TaggedString> names = defs.Select(m => m.LabelCap).ToList();
        return names.Count switch {
            0 => "",
            1 => names[0],
            2 => $"{names[0]} and {names[1]}",
            _ => string.Join(", ", names.Take(names.Count - 1)) + " and " + names.Last(),
        };
    }
}