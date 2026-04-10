using Cosmere.Core.Ability;
using Cosmere.System.Roshar.Surgebinding.Thing;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Transportation;

public class Portal : SurgebindingAbility {
    private SurgePortal? portal;
    private Map? sourceMap;
    private List<Pawn>? selectedPawns;

    internal static int pendingPortalTile = -1;
    internal static Portal? activePortal;

    public Portal(Pawn pawn) : base(pawn) { }
    public Portal(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    public override float GetStrength(Status? desiredStatus = null) {
        return base.GetStrength(desiredStatus) * (0.5f + gene.currentIdeal * 0.5f);
    }

    private ThingDef PortalDef => DefDatabase<ThingDef>.GetNamed("Cosmere_Roshar_Thing_SurgePortal");

    protected override void OnEnable() {
        base.OnEnable();
        sourceMap = pawn.Map;
        activePortal = this;
        pendingPortalTile = -1;

        Find.WorldRoutePlanner.Stop();
        Dialog_FormCaravan dialog = new Dialog_FormCaravan(sourceMap, false, OnDialogClosed);
        Find.WindowStack.Add(dialog);
    }

    private void OnDialogClosed() {
        if (!status.isActive) return;
        if (selectedPawns != null && selectedPawns.Count > 0) return;

        UpdateStatus(Active.Off);
    }

    protected override void OnDisable() {
        base.OnDisable();

        if (portal != null && !portal.Destroyed) portal.Destroy();
        portal = null;
        sourceMap = null;
        selectedPawns = null;
        pendingPortalTile = -1;
        activePortal = null;
    }

    public override IEnumerable<Command> GetGizmos() {
        foreach (Command gizmo in base.GetGizmos())
            yield return gizmo;
    }

    private void SpawnPortal() {
        if (sourceMap == null) return;

        portal = (SurgePortal)ThingMaker.MakeThing(PortalDef);
        GenSpawn.Spawn(portal, pawn.Position, sourceMap);
        FleckMaker.Static(pawn.Position, sourceMap, FleckDefOf.PsycastAreaEffect);
    }

    private bool worldTargetingActive;

    private void BeginWorldTargeting() {
        CameraJumper.TryShowWorld();
        worldTargetingActive = true;

        Find.WorldTargeter.BeginTargeting(
            (GlobalTargetInfo t) => {
                worldTargetingActive = false;
                return OnWorldTileSelected(t);
            },
            true,
            null,
            true,
            null,
            (GlobalTargetInfo t) => {
                PlanetTile tile = t.Tile;
                if (!Find.WorldGrid.InBounds(tile.tileId)) return "Out of bounds";
                return "Select portal destination";
            }
        );
    }

    public override void AbilityTick() {
        base.AbilityTick();
        if (!status.isActive) return;

        if (worldTargetingActive && !Find.WorldTargeter.IsTargeting) {
            worldTargetingActive = false;
            UpdateStatus(Active.Off);
        }
    }

    private bool OnWorldTileSelected(GlobalTargetInfo target) {
        PlanetTile tile = target.Tile;
        if (!Find.WorldGrid.InBounds(tile.tileId)) return false;

        float cost = 30f / (1 << gene.currentIdeal);
        if (!gene.CanLowerReserve(cost)) {
            Messages.Message("Not enough Stormlight", MessageTypeDefOf.RejectInput);
            return false;
        }

        gene.RemoveFromReserve(cost);

        if (selectedPawns != null && selectedPawns.Count > 0) {
            foreach (Pawn p in selectedPawns) {
                if (p.Spawned) p.DeSpawn();
            }

            Caravan caravan = CaravanMaker.MakeCaravan(selectedPawns, pawn.Faction, tile.tileId, true);

            Messages.Message(
                $"{pawn.NameShortColored} opened a portal, transporting {selectedPawns.Count} pawn(s)",
                caravan,
                MessageTypeDefOf.PositiveEvent
            );
        }

        UpdateStatus(Active.Off);
        return true;
    }

    internal void OnCaravanConfirmed(List<Pawn> pawns, PlanetTile destinationTile) {
        selectedPawns = [..pawns];
        SpawnPortal();

        if (destinationTile.Valid) {
            OnWorldTileSelected(new GlobalTargetInfo(destinationTile));
        } else {
            BeginWorldTargeting();
        }
    }

    internal void ActivateFromCaravan(Caravan caravan) {
        activePortal = this;

        Find.WorldTargeter.BeginTargeting(
            (GlobalTargetInfo t) => {
                PlanetTile tile = t.Tile;
                if (!Find.WorldGrid.InBounds(tile.tileId)) return false;

                float cost = 30f / (1 << gene.currentIdeal);
                if (!gene.CanLowerReserve(cost)) {
                    Messages.Message("Not enough Stormlight", MessageTypeDefOf.RejectInput);
                    return false;
                }

                gene.RemoveFromReserve(cost);

                Map? destinationMap = null;
                List<Map> maps = Find.Maps;
                for (int i = 0; i < maps.Count; i++) {
                    if (maps[i].Tile == tile) {
                        destinationMap = maps[i];
                        break;
                    }
                }

                if (destinationMap != null) {
                    CaravanEnterMapUtility.Enter(
                        caravan, destinationMap,
                        CaravanEnterMode.Center,
                        CaravanDropInventoryMode.DoNotDrop,
                        false
                    );
                } else {
                    caravan.pather.StopDead();
                    caravan.Tile = tile;
                    caravan.Notify_Teleported();
                }

                Messages.Message(
                    $"{pawn.NameShortColored} opened a portal, transporting the caravan",
                    destinationMap != null ? (LookTargets)destinationMap.Parent : caravan,
                    MessageTypeDefOf.PositiveEvent
                );

                activePortal = null;
                return true;
            },
            true,
            null,
            true,
            null,
            (GlobalTargetInfo t) => {
                PlanetTile tile = t.Tile;
                if (!Find.WorldGrid.InBounds(tile.tileId)) return "Out of bounds";
                return "Open portal to this location";
            }
        );
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_References.Look(ref portal, "portal");
        Scribe_References.Look(ref sourceMap, "sourceMap");
    }
}

[HarmonyPatch(typeof(CaravanFormingUtility), nameof(CaravanFormingUtility.StartFormingCaravan))]
public static class PortalCaravanInterceptPatch {
    private static bool Prefix(List<Pawn> pawns, PlanetTile destinationTile) {
        if (Portal.activePortal == null) return true;

        Portal.activePortal.OnCaravanConfirmed(pawns, destinationTile);
        return false;
    }
}

[HarmonyPatch(typeof(WorldRoutePlanner), nameof(WorldRoutePlanner.GetTicksToWaypoint))]
public static class PortalRouteTimePatch {
    private static void Postfix(ref int __result) {
        if (Portal.activePortal != null) __result = 0;
    }
}

[HarmonyPatch(typeof(Caravan), nameof(Caravan.GetGizmos))]
[StaticConstructorOnStartup]
public static class PortalCaravanGizmoPatch {
    private static readonly Texture2D PortalIcon =
        ContentFinder<Texture2D>.Get("UI/Icons/Abilities/Portal", false) ?? BaseContent.BadTex;

    private static void Postfix(Caravan __instance, ref IEnumerable<Verse.Gizmo> __result) {
        List<Verse.Gizmo> extra = [];
        List<Pawn> pawns = __instance.PawnsListForReading;
        for (int i = 0; i < pawns.Count; i++) {
            Pawn p = pawns[i];
            if (p.abilities == null) continue;

            List<RimWorld.Ability> abilities = p.abilities.AllAbilitiesForReading;
            for (int j = 0; j < abilities.Count; j++) {
                if (abilities[j] is not Portal portalAbility) continue;

                Portal captured = portalAbility;
                extra.Add(new Command_Action {
                    defaultLabel = "Portal: " + p.LabelShort,
                    defaultDesc = "Open a portal to teleport this caravan to another location instantly.",
                    icon = PortalIcon,
                    action = () => {
                        if (Portal.activePortal != null && !Find.WorldTargeter.IsTargeting)
                            Portal.activePortal = null;
                        if (Portal.activePortal != null) return;
                        captured.ActivateFromCaravan(__instance);
                    },
                });
                break;
            }
        }

        if (extra.Count > 0) {
            __result = __result.Concat(extra);
        }
    }
}
