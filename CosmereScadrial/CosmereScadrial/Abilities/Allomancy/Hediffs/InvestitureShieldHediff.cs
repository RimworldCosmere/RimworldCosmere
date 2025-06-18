using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using RimWorld;
using Verse;

namespace CosmereScadrial.Abilities.Allomancy.Hediffs;

public class InvestitureShieldHediff : AllomanticHediff {
    private MetalReserves reserves => pawn.GetComp<MetalReserves>();

    public override void Tick() {
        base.Tick();

        MetallicArtsMetalDef? metal = sourceAbilities.FirstOrDefault()?.metal;
        if (metal == null) return;

        List<MetallicArtsMetalDef> metalsToWipe = reserves.GetAllAvailableMetals().ToList();
        if (metal == MetallicArtsMetalDefOf.Aluminum) {
            metalsToWipe.Remove(metal);
        }

        if (metalsToWipe.Count == 0) return;

        foreach (MetallicArtsMetalDef? metalToWipe in metalsToWipe) {
            reserves.RemoveReserve(metalToWipe);
        }

        Messages.Message(
            GetMessage(metal, metalsToWipe),
            MessageTypeDefOf.NeutralEvent,
            false
        );
    }

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