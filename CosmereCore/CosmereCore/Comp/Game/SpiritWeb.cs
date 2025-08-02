using System;
using Cosmere.Core.Entity;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cosmere.Core.Comp.Game;

public record Connection : IExposable {
    public const float STARTING_CONNECTION = 0.25f;

    public ILoadReferenceable objectOne;
    public ILoadReferenceable objectTwo;
    private float valueInt;

    public Connection(ILoadReferenceable objectOne, ILoadReferenceable objectTwo) {
        this.objectOne = objectOne;
        this.objectTwo = objectTwo;
    }

    public Connection(ILoadReferenceable objectOne, ILoadReferenceable objectTwo, float value = 0) {
        this.objectOne = objectOne;
        this.objectTwo = objectTwo;
        this.value = value;
    }

    public float value {
        get => valueInt;
        set => valueInt = Math.Max(0, Math.Min(1, value));
    }

    public bool objectOneIsThing => objectOne is Verse.Thing;
    public bool objectTwoIsThing => objectTwo is Verse.Thing;
    public bool objectOneIsPlanetLayer => objectOne is PlanetLayer;
    public bool objectTwoIsPlanetLayer => objectTwo is PlanetLayer;
    public bool objectOneIsFaction => objectOne is Faction;
    public bool objectTwoIsFaction => objectTwo is Faction;
    public bool objectOneIsShard => objectOne is Shard;
    public bool objectTwoIsShard => objectTwo is Shard;
    public Verse.Thing? thingOne => objectOne as Verse.Thing;
    public Verse.Thing? thingTwo => objectTwo as Verse.Thing;
    public PlanetLayer? planetLayerOne => objectOne as PlanetLayer;
    public PlanetLayer? planetLayerTwo => objectTwo as PlanetLayer;
    public Faction? factionOne => objectOne as Faction;
    public Faction? factionTwo => objectTwo as Faction;
    public Shard? shardOne => objectOne as Shard;
    public Shard? shardTwo => objectTwo as Shard;

    public virtual bool Equals(Connection other) {
        return objectOne.GetUniqueLoadID() == other.objectOne.GetUniqueLoadID() &&
               objectTwo.GetUniqueLoadID() == other.objectTwo.GetUniqueLoadID();
    }

    public void ExposeData() {
        Scribe_Values.Look(ref valueInt, "value");
        Scribe_References.Look(ref objectOne, "objectOne");
        Scribe_References.Look(ref objectTwo, "objectTwo");
    }

    public virtual bool Equals(ILoadReferenceable targetOne, ILoadReferenceable targetTwo) {
        return objectOne.GetUniqueLoadID() == targetOne.GetUniqueLoadID() &&
               objectTwo.GetUniqueLoadID() == targetTwo.GetUniqueLoadID();
    }

    public bool OneObjectMatches(ILoadReferenceable target) {
        return objectOne.GetUniqueLoadID() == target.GetUniqueLoadID() ||
               objectTwo.GetUniqueLoadID() == target.GetUniqueLoadID();
    }
}

public class SpiritWeb(Verse.Game game) : GameComponent {
    private List<Connection> connectionList = [];

    [Unsaved]
    private Dictionary<(string, string), Connection> connections =
        new Dictionary<(string, string), Connection>();

    public static SpiritWeb Instance => Current.Game.GetComponent<SpiritWeb>();

    private static (string, string) NormalizeKey(ILoadReferenceable one, ILoadReferenceable two) {
        return string.CompareOrdinal(one.GetUniqueLoadID(), two.GetUniqueLoadID()) < 0
            ? (one.GetUniqueLoadID(), two.GetUniqueLoadID())
            : (two.GetUniqueLoadID(), one.GetUniqueLoadID());
    }

    public Connection GetConnection(ILoadReferenceable targetOne, ILoadReferenceable targetTwo) {
        (string, string) key = NormalizeKey(targetOne, targetTwo);
        if (!connections.TryGetValue(key, out Connection? value)) {
            connections[key] = new Connection(targetOne, targetTwo);
        }

        return connections[key];
    }

    public Connection SetConnectionValue(Connection connection, float value) {
        float oldValue = connection.value;
        connection.value = value;
        if (connection.objectOneIsThing) {
            if (connection.objectTwoIsThing) {
                connection.thingOne!.Notify_SignalReceived(
                    new Signal("cosmere_connectionChanged", connection.thingTwo, value, oldValue)
                );
            } else if (connection.objectTwoIsFaction) {
                connection.thingOne!.Notify_SignalReceived(
                    new Signal("cosmere_connectionChanged", connection.factionTwo, value, oldValue)
                );
            } else {
                connection.thingOne!.Notify_SignalReceived(
                    new Signal("cosmere_connectionChanged", connection.objectTwo.GetUniqueLoadID(), value, oldValue)
                );
            }
        }

        if (connection.objectTwoIsThing) {
            connection.thingTwo!.Notify_SignalReceived(
                new Signal("cosmere_connectionChanged", connection.objectOne.GetUniqueLoadID(), value, oldValue)
            );
        }

        return connection;
    }

    public Connection SetConnection(ILoadReferenceable targetOne, ILoadReferenceable targetTwo, float value) {
        Connection conn = GetConnection(targetOne, targetTwo);

        return SetConnectionValue(conn, value);
    }

    public Connection AdjustConnection(ILoadReferenceable targetOne, ILoadReferenceable targetTwo, float delta) {
        Connection conn = GetConnection(targetOne, targetTwo);

        return SetConnectionValue(conn, conn.value + delta);
    }

    public IEnumerable<Connection> GetConnections(ILoadReferenceable target) {
        foreach (Connection? connection in connections.Values) {
            if (connection.OneObjectMatches(target)) yield return connection;
        }
    }

    public float GetConnectionValue(ILoadReferenceable a, ILoadReferenceable b) {
        return GetConnection(a, b).value;
    }

    public bool HasConnection(ILoadReferenceable a, ILoadReferenceable b) {
        return connections.ContainsKey(NormalizeKey(a, b));
    }

    public override void ExposeData() {
        base.ExposeData();

        if (Scribe.mode == LoadSaveMode.Saving) {
            connectionList = connections.Values.Where(c => c.value != 0f).ToList();
        }

        Scribe_Collections.Look(ref connectionList, "connections", LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit) {
            connections = new Dictionary<(string, string), Connection>();
            foreach (Connection? conn in connectionList) {
                (string, string) key = NormalizeKey(conn.objectOne, conn.objectTwo);
                connections[key] = conn;
            }
        }
    }
}

public static class SpiritWebExtensions {
    public static Connection GetConnection(this Verse.Thing self, ILoadReferenceable target) {
        return SpiritWeb.Instance.GetConnection(target, self);
    }

    public static Connection GetConnection(this Faction self, ILoadReferenceable target) {
        return SpiritWeb.Instance.GetConnection(target, self);
    }

    public static Connection GetConnection(this PlanetLayer self, ILoadReferenceable target) {
        return SpiritWeb.Instance.GetConnection(target, self);
    }

    public static Connection GetConnection(this Shard self, ILoadReferenceable target) {
        return SpiritWeb.Instance.GetConnection(target, self);
    }

    public static Connection InitializeConnection(this Verse.Thing self, ILoadReferenceable target) {
        return SpiritWeb.Instance.SetConnection(target, self, Connection.STARTING_CONNECTION);
    }

    public static Connection InitializeConnection(this Faction self, ILoadReferenceable target) {
        return SpiritWeb.Instance.SetConnection(target, self, Connection.STARTING_CONNECTION);
    }

    public static Connection InitializeConnection(this PlanetLayer self, ILoadReferenceable target) {
        return SpiritWeb.Instance.SetConnection(target, self, Connection.STARTING_CONNECTION);
    }

    public static Connection InitializeConnection(this Shard self, ILoadReferenceable target) {
        return SpiritWeb.Instance.SetConnection(target, self, Connection.STARTING_CONNECTION);
    }

    public static Connection SetConnection(this Verse.Thing self, ILoadReferenceable target, float value) {
        return SpiritWeb.Instance.SetConnection(target, self, value);
    }

    public static Connection SetConnection(this Faction self, ILoadReferenceable target, float value) {
        return SpiritWeb.Instance.SetConnection(target, self, value);
    }

    public static Connection SetConnection(this PlanetLayer self, ILoadReferenceable target, float value) {
        return SpiritWeb.Instance.SetConnection(target, self, value);
    }

    public static Connection SetConnection(this Shard self, ILoadReferenceable target, float value) {
        return SpiritWeb.Instance.SetConnection(target, self, value);
    }

    public static Connection AdjustConnection(this Verse.Thing self, ILoadReferenceable target, float delta) {
        return SpiritWeb.Instance.AdjustConnection(target, self, delta);
    }

    public static Connection AdjustConnection(this Faction self, ILoadReferenceable target, float delta) {
        return SpiritWeb.Instance.AdjustConnection(target, self, delta);
    }

    public static Connection AdjustConnection(this PlanetLayer self, ILoadReferenceable target, float delta) {
        return SpiritWeb.Instance.AdjustConnection(target, self, delta);
    }

    public static Connection AdjustConnection(this Shard self, ILoadReferenceable target, float delta) {
        return SpiritWeb.Instance.AdjustConnection(target, self, delta);
    }
}