using RimWorld;
using Verse;

namespace Cosmere.Core.Comp.Game;

#pragma warning disable CS9113 // Parameter 'game' is unread - required by GameComponent base class
public class SpiritWeb(Verse.Game game) : GameComponent {
#pragma warning restore CS9113
    public const string CHANGED_SIGNAL = "Cosmere_Connection_Changed";
    private List<Connection> connectionList = [];

    private Dictionary<(string, string), Connection> connections =
        new Dictionary<(string, string), Connection>();

    public static SpiritWeb? Instance => Current.Game?.GetComponent<SpiritWeb>();

    private static (string, string) NormalizeKey(ILoadReferenceable one, ILoadReferenceable two) {
        return string.CompareOrdinal(one.GetUniqueLoadID(), two.GetUniqueLoadID()) < 0
            ? (one.GetUniqueLoadID(), two.GetUniqueLoadID())
            : (two.GetUniqueLoadID(), one.GetUniqueLoadID());
    }

    public Connection GetOrCreateConnection(ILoadReferenceable targetOne, ILoadReferenceable targetTwo) {
        (string, string) key = NormalizeKey(targetOne, targetTwo);
        if (!connections.TryGetValue(key, out Connection? _)) {
            connections[key] = InitializeConnection(targetOne, targetTwo);
        }

        return connections[key];
    }

    public Connection InitializeConnection(ILoadReferenceable targetOne, ILoadReferenceable targetTwo) {
        bool canBondObjectOne = true;
        bool canBondObjectTwo = true;
        if (targetOne is Verse.Thing thingOne && thingOne.def.HasModExtension<DefModExtension.Connection>()) {
            DefModExtension.Connection modExtension = thingOne.def.GetModExtension<DefModExtension.Connection>();
            canBondObjectOne = modExtension.canBond;
        }

        if (targetTwo is Verse.Thing thingTwo && thingTwo.def.HasModExtension<DefModExtension.Connection>()) {
            DefModExtension.Connection modExtension = thingTwo.def.GetModExtension<DefModExtension.Connection>();
            canBondObjectTwo = modExtension.canBond;
        }

        (string, string) key = NormalizeKey(targetOne, targetTwo);
        connections[key] = new Connection(targetOne, targetTwo, canBondObjectOne, canBondObjectTwo);

        return connections[key];
    }

    public Connection SetConnectionValue(Connection connection, float value) {
        float oldValue = connection.value;
        connection.value = value;
        if (connection.objectOneIsThing) {
            if (connection.objectTwoIsThing) {
                connection.thingOne!.Notify_SignalReceived(
                    new Signal(CHANGED_SIGNAL, connection.thingTwo, value, oldValue)
                );
            } else if (connection.objectTwoIsFaction) {
                connection.thingOne!.Notify_SignalReceived(
                    new Signal(CHANGED_SIGNAL, connection.factionTwo, value, oldValue)
                );
            } else {
                connection.thingOne!.Notify_SignalReceived(
                    new Signal(CHANGED_SIGNAL, connection.objectTwo.GetUniqueLoadID(), value, oldValue)
                );
            }
        }

        if (connection.objectTwoIsThing) {
            if (connection.objectOneIsThing) {
                connection.thingTwo!.Notify_SignalReceived(
                    new Signal(CHANGED_SIGNAL, connection.thingOne, value, oldValue)
                );
            } else if (connection.objectOneIsFaction) {
                connection.thingTwo!.Notify_SignalReceived(
                    new Signal(CHANGED_SIGNAL, connection.factionOne, value, oldValue)
                );
            } else {
                connection.thingTwo!.Notify_SignalReceived(
                    new Signal(CHANGED_SIGNAL, connection.objectOne.GetUniqueLoadID(), value, oldValue)
                );
            }
        }

        return connection;
    }

    public Connection SetConnection(ILoadReferenceable targetOne, ILoadReferenceable targetTwo, float value) {
        Connection conn = GetOrCreateConnection(targetOne, targetTwo);

        return SetConnectionValue(conn, value);
    }

    public Connection AdjustConnection(ILoadReferenceable targetOne, ILoadReferenceable targetTwo, float delta) {
        Connection conn = GetOrCreateConnection(targetOne, targetTwo);

        return SetConnectionValue(conn, conn.value + delta);
    }

    public IEnumerable<Connection> GetConnections(ILoadReferenceable target) {
        foreach (Connection? connection in connections.Values) {
            if (connection.OneObjectMatches(target)) yield return connection;
        }
    }

    public float GetConnectionValue(ILoadReferenceable a, ILoadReferenceable b) {
        return GetOrCreateConnection(a, b).value;
    }

    public bool HasConnection(ILoadReferenceable a, ILoadReferenceable b) {
        return connections.ContainsKey(NormalizeKey(a, b));
    }

    public override void ExposeData() {
        base.ExposeData();

        if (Scribe.mode == LoadSaveMode.Saving) {
            connectionList = connections.Values.Where(c => c.ShouldSave()).ToList();
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
