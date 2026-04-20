using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.Core.Patch;

[HarmonyPatch(typeof(Page_ChooseIdeoPreset), nameof(Page_ChooseIdeoPreset.PostOpen))]
public static class ForcePlayerFixedIdeoPatch {
    public static void Postfix(Page_ChooseIdeoPreset __instance) {
        Faction? player = Faction.OfPlayer;
        FactionDef? def = player?.def;
        if (def == null || !def.fixedIdeo) return;

        IdeoGenerationParms parms = new IdeoGenerationParms(
            def,
            forceNoExpansionIdeo: false,
            disallowedPrecepts: null,
            disallowedMemes: null,
            name: def.ideoName,
            styles: def.styles,
            deities: def.deityPresets,
            hidden: def.hiddenIdeo,
            description: def.ideoDescription,
            forcedMemes: def.forcedMemes,
            classicExtra: false,
            forceNoWeaponPreference: false,
            forNewFluidIdeo: false,
            fixedIdeo: true,
            requiredPreceptsOnly: def.requiredPreceptsOnly
        );

        player!.ideos.ChooseOrGenerateIdeo(parms);
        Ideo? forcedIdeo = player.ideos.PrimaryIdeo;
        if (forcedIdeo == null) return;

        AccessTools.Field(typeof(Page_ChooseIdeoPreset), "classicIdeo").SetValue(__instance, forcedIdeo);
        Find.IdeoManager.RemoveUnusedStartingIdeos();
        Logger.Info($"ForcePlayerFixedIdeoPatch: forced player ideo to '{forcedIdeo.name}' from fixedIdeo on {def.defName}");
    }
}
