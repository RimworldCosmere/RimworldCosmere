using System;
using System.Reflection;
using RimWorld;
using Verse;

namespace Cosmere.Core.Util;

/// <summary>
///     Writing to a pawn's Time-type records.
/// </summary>
/// <remarks>
///     Pawn_RecordsTracker.AddTo handles Int and Float only - hand it a Time record and it logs
///     an error and changes nothing. Vanilla advances Time records exclusively from its own
///     ticker, straight into the private records DefMap, so backdating one means going in the
///     same way.
/// </remarks>
public static class RecordUtility {
    private static readonly FieldInfo? RecordsField = typeof(Pawn_RecordsTracker)
        .GetField("records", BindingFlags.NonPublic | BindingFlags.Instance);

    private static PropertyInfo? indexer;

    /// <summary>Raises a record to <paramref name="value" />, never lowering an existing one.</summary>
    /// <returns>False when reflection could not reach the map, so callers can log once.</returns>
    public static bool RaiseTo(Pawn pawn, RecordDef record, float value) {
        if (pawn.records == null) return false;
        if (record.type != RecordType.Time) {
            if (pawn.records.GetValue(record) < value) {
                pawn.records.AddTo(record, value - pawn.records.GetValue(record));
            }

            return true;
        }

        object? map = RecordsField?.GetValue(pawn.records);
        if (map == null) return false;

        // DefMap carries two Item indexers - one keyed by def, one by list position - so asking
        // for "Item" by name alone throws AmbiguousMatchException. Take the def-keyed one.
        if (indexer == null) {
            foreach (PropertyInfo candidate in map.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                ParameterInfo[] args = candidate.GetIndexParameters();
                if (args.Length != 1 || args[0].ParameterType == typeof(int)) continue;
                indexer = candidate;
                break;
            }
        }

        if (indexer == null) return false;

        object[] key = [record];
        float have = Convert.ToSingle(indexer.GetValue(map, key));
        if (have >= value) return true;

        indexer.SetValue(map, value, key);
        return true;
    }
}
