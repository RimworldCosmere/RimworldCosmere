using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Comps.Things;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Patches {
    [HarmonyPatch(typeof(FloatMenuMakerMap), nameof(FloatMenuMakerMap.ChoicesAtFor))]
    public static class RightClickMenu {
        public static void Postfix(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> __result) {
            if (!pawn.TryGetComp<RightClickAbilities>(out var comp)) return;

            var target = GenUI.TargetsAt(clickPos, TargetingParameters.ForPawns(), true).FirstOrDefault();
            if (!target.IsValid) return;

            foreach (var ability in comp.rightClickAbilities.Where(ability => ability.CanUse(pawn, target))) {
                __result.Add(new FloatMenuOption(ability.def.label, () => ability.Execute(pawn, target), ability.def.Icon, Color.white));
            }
        }
    }
}