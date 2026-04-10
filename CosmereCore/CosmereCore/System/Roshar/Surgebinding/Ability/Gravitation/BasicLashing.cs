using Cosmere.Core.Ability;
using Cosmere.System.Roshar.Surgebinding.Utility;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Gravitation;

public class BasicLashing : SurgebindingAbility {
    public static readonly HashSet<Pawn> FlyingPawns = [];

    private const int GroupRadius = 5;
    private readonly List<Pawn> lashedAllies = [];
    private bool worldTargetingActive;

    public BasicLashing(Pawn pawn) : base(pawn) { }
    public BasicLashing(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    public override float GetStrength(Status? desiredStatus = null) {
        return base.GetStrength(desiredStatus) * (0.5f + gene.currentIdeal * 0.5f);
    }

    protected override void OnEnable() {
        base.OnEnable();
        FlyingPawns.Add(pawn);
        SurgebindingHediffUtility.GetOrAddHediff(pawn, this, def.hediff);
    }

    protected override void OnDisable() {
        base.OnDisable();
        FlyingPawns.Remove(pawn);

        for (int i = lashedAllies.Count - 1; i >= 0; i--) {
            Pawn ally = lashedAllies[i];
            if (ally != null && !ally.Dead) {
                FlyingPawns.Remove(ally);
                SurgebindingHediffUtility.RemoveHediff(ally, this, def.hediff);
            }
        }

        lashedAllies.Clear();
    }

    public override void AbilityTick() {
        base.AbilityTick();
        if (!status.isActive) return;
        if (!pawn.Spawned) return;

        if (gene.currentIdeal >= 2) {
            UpdateGroupLashing();
        }
    }

    private void UpdateGroupLashing() {
        for (int i = lashedAllies.Count - 1; i >= 0; i--) {
            Pawn ally = lashedAllies[i];
            if (ally == null || ally.Dead || !ally.Position.InHorDistOf(pawn.Position, GroupRadius)) {
                if (ally != null && !ally.Dead) {
                    FlyingPawns.Remove(ally);
                    SurgebindingHediffUtility.RemoveHediff(ally, this, def.hediff);
                }

                lashedAllies.RemoveAt(i);
            }
        }

        if (!pawn.IsHashIntervalTick(30)) return;

        foreach (Verse.Thing thing in GenRadial.RadialDistinctThingsAround(
                     pawn.Position, pawn.Map, GroupRadius, true
                 )) {
            if (thing is not Pawn ally) continue;
            if (ally == pawn || ally.Dead) continue;
            if (ally.Faction != pawn.Faction) continue;
            if (lashedAllies.Contains(ally)) continue;

            FlyingPawns.Add(ally);
            SurgebindingHediffUtility.GetOrAddHediff(ally, this, def.hediff);
            lashedAllies.Add(ally);
        }
    }

    public override IEnumerable<Command> GetGizmos() {
        foreach (Command gizmo in base.GetGizmos()) {
            yield return gizmo;
        }

        if (!status.isActive) yield break;
        if (gene.currentIdeal < 2) yield break;

        Command_Action flyToCommand = new Command_Action {
            defaultLabel = "Fly To...",
            defaultDesc = "Lash yourself and nearby allies to a destination on the world map, arriving instantly.",
            icon = def.uiIcon,
            action = BeginWorldFlight,
        };
        yield return flyToCommand;
    }

    private void BeginWorldFlight() {
        CameraJumper.TryShowWorld();

        Find.WorldTargeter.BeginTargeting(
            OnWorldTileSelected,
            true,
            null,
            true,
            null,
            (GlobalTargetInfo t) => {
                PlanetTile tile = t.Tile;
                if (!Find.WorldGrid.InBounds(tile.tileId)) return "Out of bounds";
                return "Fly to this location";
            }
        );
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

        List<Pawn> flyingGroup = [pawn];
        flyingGroup.AddRange(lashedAllies.Where(a => a != null && !a.Dead && a.Spawned));

        foreach (Pawn p in flyingGroup) {
            if (p.Spawned) p.DeSpawn();
        }

        Caravan caravan = CaravanMaker.MakeCaravan(flyingGroup, pawn.Faction, tile.tileId, true);

        UpdateStatus(Active.Off);

        Messages.Message(
            $"{pawn.NameShortColored} lashed {flyingGroup.Count} pawn(s) to a new destination",
            caravan,
            MessageTypeDefOf.PositiveEvent
        );

        return true;
    }
}
