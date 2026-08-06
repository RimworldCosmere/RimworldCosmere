using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Grid;

/// <summary>
///     What each cell was before the ash took it, so the Catacendre can hand the map back. Held
///     apart from TerrainGrid because the ash terrain is not layerable, so vanilla keeps no
///     under-layer for it and there would be nothing to restore from.
/// </summary>
public class AshTerrainMemory : IExposable {
    private static Dictionary<ushort, TerrainDef>? byHash;

    private readonly Verse.Map map;

    /// <summary>
    ///     Short hash of the pre-ash terrain, zero for untouched. One flat ushort per cell rather
    ///     than a dictionary: 125 KB fixed beats 62,500 boxed entries on a fully buried map, and
    ///     it is the same encoding TerrainGrid itself saves in. Null until the first swap.
    /// </summary>
    private ushort[]? original;

    public AshTerrainMemory(Verse.Map map) {
        this.map = map;
    }

    /// <summary>Cells still standing as ash. Zero means the map owes nothing back.</summary>
    public int SwappedCount { get; private set; }

    public bool IsSwapped(int index) {
        return original != null && original[index] != 0;
    }

    public void Remember(int index, TerrainDef terrain) {
        original ??= new ushort[map.cellIndices.NumGridCells];
        if (original[index] != 0) return;

        original[index] = terrain.shortHash;
        SwappedCount++;
    }

    /// <summary>Hands the original back and forgets the cell. Null if nothing was recorded.</summary>
    public TerrainDef? Take(int index) {
        if (original == null) return null;

        ushort hash = original[index];
        if (hash == 0) return null;

        original[index] = 0;
        SwappedCount--;

        // Drop the array the moment the last cell reverts, so a drained map carries none of this
        // in memory or into the save.
        if (SwappedCount == 0) original = null;

        return Lookup(hash);
    }

    public void ExposeData() {
        bool any = SwappedCount > 0;
        Scribe_Values.Look(ref any, "ashTerrainAny", false);

        if (!any) {
            if (Scribe.mode == LoadSaveMode.LoadingVars) {
                original = null;
                SwappedCount = 0;
            }

            return;
        }

        MapExposeUtility.ExposeUshort(map, ReadCell, WriteCell, "ashTerrainOriginal");

        if (Scribe.mode == LoadSaveMode.PostLoadInit) Recount();
    }

    private ushort ReadCell(IntVec3 cell) {
        return original == null ? (ushort)0 : original[map.cellIndices.CellToIndex(cell)];
    }

    private void WriteCell(IntVec3 cell, ushort value) {
        if (value == 0) return;

        original ??= new ushort[map.cellIndices.NumGridCells];
        original[map.cellIndices.CellToIndex(cell)] = value;
    }

    private void Recount() {
        SwappedCount = 0;
        if (original == null) return;

        for (int i = 0; i < original.Length; i++) {
            if (original[i] != 0) SwappedCount++;
        }

        if (SwappedCount == 0) original = null;
    }

    /// <summary>
    ///     Soil when the def is gone, matching what TerrainGrid does for a missing hash. Leaving
    ///     the cell as ash instead would strand it there for the rest of the game.
    /// </summary>
    private static TerrainDef Lookup(ushort hash) {
        if (byHash == null) {
            byHash = new Dictionary<ushort, TerrainDef>();
            foreach (TerrainDef def in DefDatabase<TerrainDef>.AllDefs) {
                byHash[def.shortHash] = def;
            }
        }

        if (byHash.TryGetValue(hash, out TerrainDef found)) return found;

        // Cache the substitute so a mod removed mid-save reports once rather than per cell.
        Core.Logger.Error($"No terrain with short hash {hash} to restore under ash. Using soil.");
        byHash[hash] = TerrainDefOf.Soil;
        return TerrainDefOf.Soil;
    }
}
