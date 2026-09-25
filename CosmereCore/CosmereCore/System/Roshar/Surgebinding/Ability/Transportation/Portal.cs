using Concord;
using Cosmere.Core.Ability;
using Cosmere.System.Roshar.Surgebinding.Thing;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Transportation;

public class Portal : SurgebindingAbility {
    internal static readonly Dictionary<int, Portal> activePortals = [];
    private SurgePortal? portal;
    private List<Pawn>? selectedPawns;
    private Map? sourceMap;

    private bool worldTargetingActive;

    public Portal(Pawn pawn) : base(pawn) { }

    public Portal(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    private static ThingDef PortalDef => ThingDefOf.Cosmere_Roshar_Thing_SurgePortal;

    public override float GetStrength(Status? desiredStatus = null) {
        return base.GetStrength(desiredStatus) * (0.5f + Gene.CurrentIdeal * 0.5f);
    }

    protected override void OnEnable() {
        base.OnEnable();
        sourceMap = pawn.Map;
        activePortals[pawn.thingIDNumber] = this;

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
                if (!Find.WorldGrid.InBounds(tile.tileId)) return "CRO_World_OutOfBounds".Translate();
                return "CRO_Portal_SelectDestination".Translate();
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
            Messages.Message("CRO_Surgebinding_NotEnoughStormlight".Translate(), MessageTypeDefOf.RejectInput);
            return false;
        }

        Gene.RemoveFromReserve(cost);

        if (selectedPawns != null && selectedPawns.Count > 0) {
            foreach (Pawn p in selectedPawns) {
                if (p.Spawned) p.DeSpawn();
            }

            Caravan caravan = CaravanMaker.MakeCaravan(selectedPawns, pawn.Faction, tile.tileId, true);

            Messages.Message(
                "CRO_Portal_PawnsMoved".Translate(
                    pawn.NameShortColored.Named("PAWN"),
                    selectedPawns.Count.Named("COUNT")
                ),
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
            t => MoveCaravanThroughPortal(caravan, t),
            true,
            null,
            true,
            null,
            t => {
                PlanetTile tile = t.Tile;
                if (!Find.WorldGrid.InBounds(tile.tileId)) return "CRO_World_OutOfBounds".Translate();
                return "CRO_Portal_OpenHere".Translate();
            }
        );
    }

    private bool MoveCaravanThroughPortal(Caravan caravan, GlobalTargetInfo target) {
        PlanetTile tile = target.Tile;
        if (!Find.WorldGrid.InBounds(tile.tileId)) return false;

        float cost = 30f / (1 << Gene.CurrentIdeal);
        if (!Gene.CanLowerReserve(cost)) {
            Messages.Message("CRO_Surgebinding_NotEnoughStormlight".Translate(), MessageTypeDefOf.RejectInput);
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
            "CRO_Portal_CaravanMoved".Translate(pawn.NameShortColored.Named("PAWN")),
            destinationMap != null ? (LookTargets)destinationMap.Parent : caravan,
            MessageTypeDefOf.PositiveEvent
        );

        activePortals.Remove(pawn.thingIDNumber);
        return true;
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
            Verse.Gizmo? gizmo = MakePortalGizmo(pawns[i], self);
            if (gizmo != null) extra.Add(gizmo);
        }

        ch.ReturnValue = ch.ReturnValue.Concat(extra);
    }

    private static Verse.Gizmo? MakePortalGizmo(Pawn p, Caravan caravan) {
        if (p.abilities == null) return null;

        List<RimWorld.Ability> abilities = p.abilities.AllAbilitiesForReading;
        for (int i = 0; i < abilities.Count; i++) {
            if (abilities[i] is not Portal portalAbility) continue;

            return new Command_Action {
                defaultLabel = "CRO_Portal_Caravan_Label".Translate(p.LabelShort.Named("PAWN")),
                defaultDesc = "CRO_Portal_Caravan_Desc".Translate(),
                icon = PortalIcon,
                action = () => OpenPortalForCaravan(portalAbility, caravan),
            };
        }

        return null;
    }

    private static void OpenPortalForCaravan(Portal portalAbility, Caravan caravan) {
        if (Portal.activePortals.Count > 0 && !Find.WorldTargeter.IsTargeting) {
            Portal.activePortals.Clear();
        }

        if (Portal.activePortals.Count > 0) return;
        portalAbility.ActivateFromCaravan(caravan);
    }
}
