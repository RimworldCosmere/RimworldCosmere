using Cosmere.Core.Ability;
using Cosmere.Core.Hediff;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Allomancy.Hediff;

public class InvestitureShieldHediff : AllomanticHediff {
    private bool messageSent;

    public InvestitureShieldHediff() { }

    public InvestitureShieldHediff(
        HediffDef hediffDef,
        Pawn pawn,
        IAbility<Allomancer, IHediff<Allomancer>> ability
    ) : base(
        hediffDef,
        pawn,
        ability
    ) { }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        MetallicArtsMetalDef? metal = null;
        foreach (IAbility<Allomancer, IHediff<Allomancer>> sa in SourceAbilities) {
            if (sa is AllomancyAbility allomancyAbility) {
                metal = allomancyAbility.metal;
                break;
            }
        }

        if (metal == null) return;

        List<Allomancer> genes = pawn.genes.GetAllomanticGenes();
        for (int i = genes.Count - 1; i >= 0; i--) {
            if (Mathf.Approximately(genes[i].Value, 0f)) {
                genes.RemoveAt(i);
            } else if (metal == MetallicArtsMetalDefOf.Aluminum && genes[i].metal == MetallicArtsMetalDefOf.Aluminum) {
                genes.RemoveAt(i);
            }
        }

        if (genes.Count == 0) {
            messageSent = false;
            return;
        }

        List<MetallicArtsMetalDef> metalsToWipe = [];
        for (int i = 0; i < genes.Count; i++) {
            genes[i].WipeReserve();
            metalsToWipe.Add(genes[i].metal);
        }

        if (!messageSent) {
            Messages.Message(
                GetMessage(metal, metalsToWipe),
                MessageTypeDefOf.NeutralEvent,
                false
            );
            messageSent = true;
        }
    }

    private string GetMessage(MetallicArtsMetalDef metal, List<MetallicArtsMetalDef> metalsToWipe) {
        string metalsToWipeString = FormatDefList(metalsToWipe);
        if (metal.Equals(MetallicArtsMetalDefOf.Aluminum)) {
            return "CS_WipeReserves".Translate(
                    pawn.Named("pawn"),
                    pawn.NameFullColored.Named("PAWN"),
                    metal.coloredLabel.Named("METAL"),
                    metalsToWipeString.Named("METALS")
                )
                .Resolve();
        }

        if (metal.Equals(MetallicArtsMetalDefOf.Chromium)) {
            return "CS_WipeReservesByLurcher".Translate(
                    pawn.Named("pawn"),
                    pawn.NameFullColored.Named("PAWN"),
                    metalsToWipeString.Named("METALS")
                )
                .Resolve();
        }

        return string.Empty;
    }

    public static string FormatDefList(List<MetallicArtsMetalDef> defs) {
        if (defs.Count == 0) return string.Empty;
        if (defs.Count == 1) return defs[0].LabelCap;
        if (defs.Count == 2) return $"{defs[0].LabelCap} and {defs[1].LabelCap}";

        List<string> names = [];
        for (int i = 0; i < defs.Count - 1; i++) {
            names.Add(defs[i].LabelCap);
        }

        return string.Join(", ", names) + " and " + defs[defs.Count - 1].LabelCap;
    }
}
