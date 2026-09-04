using Cosmere.Core;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Util;

public static class CasteUtility {
    private static readonly string DarkeyesHeritageDefName = "Cosmere_Roshar_Gene_DarkeyesHeritage";
    private static readonly string DarkeyesEnduranceDefName = "Cosmere_Roshar_Gene_DarkeyesEndurance";
    private static readonly string DarkeyesResilienceDefName = "Cosmere_Roshar_Gene_DarkeyesResilience";
    private static readonly string LighteyesHeritageDefName = "Cosmere_Roshar_Gene_LighteyesHeritage";
    private static readonly string LighteyesSocialDefName = "Cosmere_Roshar_Gene_LighteyesSocial";
    private static readonly string LighteyesEducationDefName = "Cosmere_Roshar_Gene_LighteyesEducation";
    private static readonly string LighteyesXenotypeDefName = "Cosmere_Roshar_Xenotype_Lighteyes";

    private static readonly string[] NahnGeneDefNames = [
        "Cosmere_Roshar_Gene_Nahn_High",
        "Cosmere_Roshar_Gene_Nahn_Mid",
        "Cosmere_Roshar_Gene_Nahn_Low",
    ];

    public static bool IsDarkeyes(Pawn pawn) {
        if (pawn.genes == null) return false;
        GeneDef? darkeyes = DefDatabase<GeneDef>.GetNamedSilentFail(DarkeyesHeritageDefName);
        return darkeyes != null && pawn.genes.HasActiveGene(darkeyes);
    }

    public static bool IsLighteyes(Pawn pawn) {
        if (pawn.genes == null) return false;
        GeneDef? lighteyes = DefDatabase<GeneDef>.GetNamedSilentFail(LighteyesHeritageDefName);
        return lighteyes != null && pawn.genes.HasActiveGene(lighteyes);
    }

    public static bool DarkeyesToLighteyes(Pawn pawn, string? dahnGene = null) {
        if (pawn.genes == null) return false;
        if (!IsDarkeyes(pawn)) return false;

        RemoveGeneByName(pawn, DarkeyesHeritageDefName);
        RemoveGeneByName(pawn, DarkeyesEnduranceDefName);
        RemoveGeneByName(pawn, DarkeyesResilienceDefName);

        for (int i = 0; i < NahnGeneDefNames.Length; i++) {
            RemoveGeneByName(pawn, NahnGeneDefNames[i]);
        }

        AddGeneByName(pawn, LighteyesHeritageDefName);
        AddGeneByName(pawn, LighteyesSocialDefName);
        AddGeneByName(pawn, LighteyesEducationDefName);

        if (dahnGene != null) {
            AddGeneByName(pawn, dahnGene);
        }

        XenotypeDef? lighteyesXenotype = DefDatabase<XenotypeDef>.GetNamedSilentFail(LighteyesXenotypeDefName);
        if (lighteyesXenotype != null) {
            pawn.genes.SetXenotype(lighteyesXenotype);
        }

        Log.Info($"CasteUtility: {pawn.NameShortColored} transitioned from darkeyes to lighteyes");
        return true;
    }

    private static void RemoveGeneByName(Pawn pawn, string defName) {
        GeneDef? geneDef = DefDatabase<GeneDef>.GetNamedSilentFail(defName);
        if (geneDef == null) return;

        Verse.Gene? gene = pawn.genes?.GetGene(geneDef);
        if (gene != null) {
            pawn.genes!.RemoveGene(gene);
        }
    }

    private static void AddGeneByName(Pawn pawn, string defName) {
        GeneDef? geneDef = DefDatabase<GeneDef>.GetNamedSilentFail(defName);
        if (geneDef == null) {
            Log.Warn($"CasteUtility: Gene '{defName}' not found");
            return;
        }

        if (!pawn.genes!.HasActiveGene(geneDef)) {
            pawn.genes.AddGene(geneDef, true);
        }
    }
}
