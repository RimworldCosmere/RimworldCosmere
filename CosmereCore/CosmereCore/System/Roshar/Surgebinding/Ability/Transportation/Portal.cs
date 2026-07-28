using Concord;
using Cosmere.Core.Ability;
using Cosmere.System.Roshar.Surgebinding.Thing;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Transportation;

public class Portal : SurgebindingAbility {
    internal static int pendingPortalTile = -1;
    internal static readonly Dictionary<int, Portal> activePortals = new Dictionary<int, Portal>();
    private SurgePortal? portal;
    private List<Pawn>? selectedPawns;
    private Map? sourceMap;

    private bool worldTargetingActive;

    public Portal(Pawn pawn) : base(pawn) { }

    public Portal(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    private ThingDef PortalDef => ThingDefOf.Cosmere_Roshar_Thing_SurgePortal;

    public override float GetStrength(Status? desiredStatus = null) {
        return base.GetStrength(desiredStatus) * (0.5f + Gene.CurrentIdeal * 0.5f);
    }

    protected override void OnEnable() {
        base.OnEnable();
        sourceMap = pawn.Map;
        activePortals[pawn.thingIDNumber] = this;
        pendingPortalTile = -1;

        Find.WorldRoutePlanner.Stop();
        Dialog_FormCaravan dialog = new Dialog_FormCaravan(sourceMap, false, OnDialogClosed);
        Find.WindowStack.Add(dialog);
    }

    private void OnDialogClosed() {
        if (!status.IsActive) return;
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
        activePortals.Remove(pawn.thingIDNumber);
    }

    public override IEnumerable<Command> GetGizmos() {
        foreach (Command gizmo in base.GetGizmos()) {
            yield return gizmo;
        }
    }

    private void SpawnPortal() {
        if (sourceMap == null) return;

        portal = (SurgePortal)ThingMaker.MakeThing(PortalDef);
        GenSpawn.Spawn(portal, pawn.Position, sourceMap);
        FleckMaker.Static(pawn.Position, sourceMap, FleckDefOf.PsycastAreaEffect);
    }

    private void BeginWorldTargeting() {
        CameraJumper.TryShowWorld();
        worldTargetingActive = true;

        Find.WorldTargeter.BeginTargeting(
            t => {
                worldTargetingActive = false;
                return OnWorldTileSelected(t);
            },
            true,
            null,
            true,
            null,
            t => {
                PlanetTile tile = t.Tile;
                if (!Find.WorldGrid.InBounds(tile.tileId)) return "Out of bounds";
                return "Select portal destination";
            }
        );
    }

    public override void AbilityTick() {
        base.AbilityTick();
        if (!status.IsActive) return;

        if (worldTargetingActive && !Find.WorldTargeter.IsTargeting) {
            worldTargetingActive = false;
            UpdateStatus(Active.Off);
        }
    }

    private bool OnWorldTileSelected(GlobalTargetInfo target) {
        PlanetTile tile = target.Tile;
        if (!Find.WorldGrid.InBounds(tile.tileId)) return false;

        float cost = 30f / (1 << Gene.CurrentIdeal);
        if (!Gene.CanLowerReserve(cost)) {
            Messages.Message("Not enough Stormlight", MessageTypeDefOf.RejectInput);
            return false;
        }

        Gene.RemoveFromReserve(cost);

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
        selectedPawns = [.. pawns];
        SpawnPortal();

        if (destinationTile.Valid) {
            OnWorldTileSelected(new GlobalTargetInfo(destinationTile));
        } else {
            BeginWorldTargeting();
        }
    }

    internal void ActivateFromCaravan(Caravan caravan) {
        activePortals[pawn.thingIDNumber] = this;

        Find.WorldTargeter.BeginTargeting(
            t => {
                PlanetTile tile = t.Tile;
                if (!Find.WorldGrid.InBounds(tile.tileId)) return false;

                float cost = 30f / (1 << Gene.CurrentIdeal);
                if (!Gene.CanLowerReserve(cost)) {
                    Messages.Message("Not enough Stormlight", MessageTypeDefOf.RejectInput);
                    return false;
                }

                Gene.RemoveFromReserve(cost);

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
                        caravan,
                        destinationMap,
                        CaravanEnterMode.Center
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

                activePortals.Remove(pawn.thingIDNumber);
                return true;
            },
            true,
            null,
            true,
            null,
            t => {
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

[Patch(typeof(CaravanFormingUtility))]
public static class PortalCaravanInterceptPatch {
    [Inject(At.Head, nameof(CaravanFormingUtility.StartFormingCaravan))]
    private static Control BeforeStartFormingCaravan(List<Pawn> pawns, PlanetTile destinationTile) {
        if (Portal.activePortals.Count == 0) return Control.Continue;

        Portal active = Portal.activePortals.Values.First();
        active.OnCaravanConfirmed(pawns, destinationTile);
        return Control.Cancel;
    }
}

[Patch]
public abstract class PortalRouteTimePatch : WorldRoutePlanner {
    [Inject(At.Return, nameof(GetTicksToWaypoint))]
    private void AfterGetTicksToWaypoint(ControlHandle<int> ch) {
        if (Portal.activePortals.Count > 0) ch.ReturnValue = 0;
    }
}

[Patch]
[StaticConstructorOnStartup]
public abstract class PortalCaravanGizmoPatch : Caravan {
    private static readonly Texture2D PortalIcon =
        ContentFinder<Texture2D>.Get("UI/Icons/Abilities/Portal", false) ?? BaseContent.BadTex;

    [Inject(At.Return, nameof(GetGizmos))]
    private void AfterGetGizmos(ControlHandle<IEnumerable<Verse.Gizmo>> ch) {
        Caravan self = this;
        List<Verse.Gizmo> extra = [];
        List<Pawn> pawns = self.PawnsListForReading;
        for (int i = 0; i < pawns.Count; i++) {
            Pawn p = pawns[i];
            if (p.abilities == null) continue;

            List<RimWorld.Ability> abilities = p.abilities.AllAbilitiesForReading;
            for (int j = 0; j < abilities.Count; j++) {
                if (abilities[j] is not Portal portalAbility) continue;

                Portal captured = portalAbility;
                extra.Add(
                    new Command_Action {
                        defaultLabel = "Portal: " + p.LabelShort,
                        defaultDesc = "Open a portal to teleport this caravan to another location instantly.",
                        icon = PortalIcon,
                        action = () => {
                            if (Portal.activePortals.Count > 0 && !Find.WorldTargeter.IsTargeting) {
                                Portal.activePortals.Clear();
                            }

                            if (Portal.activePortals.Count > 0) return;
                            captured.ActivateFromCaravan(self);
                        },
                    }
                );
                break;
            }
        }

        if (extra.Count > 0) {
            ch.ReturnValue = ch.ReturnValue.Concat(extra);
        }
    }
}
