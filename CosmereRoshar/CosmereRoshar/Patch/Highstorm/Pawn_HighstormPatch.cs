using HarmonyLib;
using Verse;

namespace Cosmere.Roshar.Patches.Highstorm;

[HarmonyPatch(typeof(Pawn))]
[HarmonyPatch("TickInterval")]
public static class PawnHighstormPushPatch {
    private static readonly string WindrunnerBondText =
        "pauses. A presence lingers at the edge of their awareness—watching, waiting. It has seen their struggles, their moments of strength, their failures. And yet, it remains.\r\n\r\nA faint whisper echoes in the back of their mind, words unbidden yet undeniable:\r\n\"Life before death. Strength before weakness. Journey before destination.\"";

    private static readonly string TruthwatcherBondText =
        "pauses. The world seems to blur at the edges, as if reality itself hesitates to be fully known. In the shifting mist, a quiet presence lingers—watching, not with eyes, but with understanding. It has seen their doubts, their curiosity, their silent strength.\r\n\r\nA whisper, clear and certain, forms within their thoughts:\r\n\"Life before death. Strength before weakness. Journey before destination.\"";

    private static readonly string EdgedancerBondText =
        "pauses. The scent of blooming life drifts on the air, and something warm brushes against their soul—a presence both nurturing and ancient. It remembers who they are, even when the world does not. It has seen their compassion, their small kindnesses, their quiet resolve.\r\n\r\nFrom somewhere deep within, words rise like new growth:\r\n\"Life before death. Strength before weakness. Journey before destination.\"";

    private static readonly string SkybreakerBondText =
        "pauses. The air grows heavy, still with judgment. A presence watches from afar, cold and unyielding—but not without purpose. It has measured their choices, their discipline, their will to obey or defy.\r\n\r\nThen, without emotion or mercy, the words arrive—etched like a sentence passed:\r\n\"Life before death. Strength before weakness. Journey before destination.\"";

    private static readonly float InitialBondMaxNumber = 100000f;

    private static void Postfix(Pawn __instance, int delta) {
        if (__instance.IsHashIntervalTick(GenTicks.TicksPerRealSecond / 3, delta)) return;

        StormShelterManager.FirstTickOfHighstorm = true;
    }

    private static bool IsHighstormActive(Map map) {
        // Checks if our custom GameCondition is present
        GameCondition.Highstorm? condition = map.gameConditionManager.GetActiveCondition<GameCondition.Highstorm>();
        return condition != null;
    }

    private static bool IsPawnValidForStorm(Pawn pawn) {
        if (pawn.Dead || pawn.Destroyed || !pawn.Spawned) return false;
        if (pawn.Map == null) return false;
        if (!pawn.Position.IsValid) return false;
        if (!pawn.Position.InBounds(pawn.Map)) return false;

        return !pawn.Position.Roofed(pawn.Map);
    }

    /*private static bool CheckIfSheltered(Pawn pawn) {
        return !pawn.ShouldBeMovedByStorm();
    }*/


    /*private static void DamageAndMovePawn(Pawn instance) {
        if (CheckIfSheltered(instance)) {
            return;
        }

        bool isRadiant = instance.story?.traits.allTraits.Any(StormlightUtilities.IsRadiant) ?? false;
        if (Mod.enableHighstormDamage || instance.RaceProps.Humanlike && instance.Faction.IsPlayer) {
            instance.TakeDamage(new DamageInfo(DamageDefOf.Blunt, 1));
        }

        // Storm always blows from east -> pushes pawns west
        IntVec3 newPos = instance.Position + IntVec3.West;
        if (!isRadiant &&
            instance.Map != null &&
            newPos.Walkable(instance.Map) &&
            newPos.IsValid &&
            newPos.InBounds(instance.Map)) {
            instance.Position = newPos;
        } else {
            Stormlight? stormlightComp = instance.TryGetComp<Stormlight>();
            if (stormlightComp != null) {
                stormlightComp.InfuseStormlight(25f); // 25 units per check
            }
        }
    }*/
}