using RimWorld;
using Verse;

namespace Cosmere.Core.Comp.Game;

#pragma warning disable CS9113 // Parameter 'game' is unread - required by GameComponent base class
public class SpiritWeb(Verse.Game game) : GameComponent {
    public const string ChangedSignal = "Cosmere_Connection_Changed";
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
        if (connections.TryGetValue(key, out Connection? existing)) {
            return existing;
        }

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

        connections[key] = new Connection(targetOne, targetTwo, canBondObjectOne, canBondObjectTwo);
        return connections[key];
    }

    public Connection? TryGetConnection(ILoadReferenceable targetOne, ILoadReferenceable targetTwo) {
        return connections.TryGetValue(NormalizeKey(targetOne, targetTwo), out Connection? c) ? c : null;
    }

    public Connection SetConnectionValue(Connection connection, float value) {
        float oldValue = connection.Value;
        connection.Value = value;
        connection.NotifyChange(value, oldValue);
        return connection;
    }

    public Connection SetConnection(ILoadReferenceable targetOne, ILoadReferenceable targetTwo, float value) {
        Connection conn = GetOrCreateConnection(targetOne, targetTwo);

        return SetConnectionValue(conn, value);
    }

    public Connection AdjustConnection(ILoadReferenceable targetOne, ILoadReferenceable targetTwo, float delta) {
        Connection conn = GetOrCreateConnection(targetOne, targetTwo);

        return SetConnectionValue(conn, conn.Value + delta);
    }

    public IEnumerable<Connection> GetConnections(ILoadReferenceable target) {
        foreach (Connection connection in connections.Values) {
            if (connection.OneObjectMatches(target)) yield return connection;
        }
    }

    public float GetConnectionValue(ILoadReferenceable targetOne, ILoadReferenceable targetTwo) {
        return TryGetConnection(targetOne, targetTwo)?.Value ?? 0f;
    }

    public bool HasConnection(ILoadReferenceable targetOne, ILoadReferenceable targetTwo) {
        return connections.ContainsKey(NormalizeKey(targetOne, targetTwo));
    }

    public override void ExposeData() {
        base.ExposeData();

        if (Scribe.mode == LoadSaveMode.Saving) {
            connectionList = connections.Values.Where(c => c.ShouldSave()).ToList();
        }

        Scribe_Collections.Look(ref connectionList, "connections", LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit) {
            connections = new Dictionary<(string, string), Connection>();
            foreach (Connection conn in connectionList) {
                (string, string) key = NormalizeKey(conn.ObjectOne, conn.ObjectTwo);
                connections[key] = conn;
            }
        }
    }
#pragma warning restore CS9113
}