using System;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Grid;

/// <summary>How deep the ash is underfoot, in bands the rest of the game reacts to.</summary>
public enum AshBucket {
    None = 0,
    Dusting = 1,
    Ankle = 2,
    Knee = 3,
    Waist = 4,
}

/// <summary>
///     Per-cell ash depth in centimetre-ish units - one byte per cell, ten millimetres a step, so
///     the whole 250x250 map costs 61 KB and still reaches the two and a half metres the end of
///     Hero of Ages needs.
/// </summary>
public class AshGrid : IExposable {
    public const int MaxDepthMm = 2550;
    public const int UnitMm = 10;

    private readonly Verse.Map map;
    private byte[] depth;

    public AshGrid(Verse.Map map) {
        this.map = map;
        depth = new byte[map.cellIndices.NumGridCells];
    }

    /// <summary>Cheap enough to poll every tick, and the only thing the render gate needs.</summary>
    public long TotalUnits { get; private set; }

    public bool Any => TotalUnits > 0;

    public void ExposeData() {
        DataExposeUtility.LookByteArray(ref depth, "ashDepth");

        if (Scribe.mode != LoadSaveMode.PostLoadInit) return;
        if (depth == null || depth.Length != map.cellIndices.NumGridCells) {
            depth = new byte[map.cellIndices.NumGridCells];
        }

        RecountTotal();
    }

    public int GetDepthMm(IntVec3 cell) {
        return depth[map.cellIndices.CellToIndex(cell)] * UnitMm;
    }

    public int GetDepthMm(int index) {
        return depth[index] * UnitMm;
    }

    /// <summary>0 to 1 against the maximum, which is what the shader and the buckets both want.</summary>
    public float GetFraction(int index) {
        return depth[index] * UnitMm / (float)MaxDepthMm;
    }

    public AshBucket GetBucket(int index) {
        return AshDepthMath.BucketFor(depth[index] * UnitMm);
    }

    /// <summary>Roofed cells never gather ash. That is the whole reason skaa build roofs.</summary>
    public bool CanHaveAsh(IntVec3 cell) {
        return !map.roofGrid.Roofed(cell);
    }

    /// <summary>Returns the millimetres actually added, which is less than asked for near the cap.</summary>
    public int AddDepthMm(int index, int millimetres) {
        byte before = depth[index];
        int after = Mathf.Clamp(before + millimetres / UnitMm, 0, MaxDepthMm / UnitMm);
        if (after == before) return 0;

        depth[index] = (byte)after;
        TotalUnits += after - before;
        return (after - before) * UnitMm;
    }

    /// <summary>Returns the millimetres actually removed, so clearing can conserve mass.</summary>
    public int RemoveDepthMm(int index, int millimetres) {
        byte before = depth[index];
        if (before == 0) return 0;

        int after = Mathf.Max(0, before - millimetres / UnitMm);
        depth[index] = (byte)after;
        TotalUnits -= before - after;
        return (before - after) * UnitMm;
    }

    public void SetDepthMm(int index, int millimetres) {
        byte before = depth[index];
        byte after = (byte)Mathf.Clamp(millimetres / UnitMm, 0, MaxDepthMm / UnitMm);
        if (after == before) return;

        depth[index] = after;
        TotalUnits += after - before;
    }

    public void Clear() {
        Array.Clear(depth, 0, depth.Length);
        TotalUnits = 0;
    }

    private void RecountTotal() {
        long total = 0;
        for (int i = 0; i < depth.Length; i++) {
            total += depth[i];
        }

        TotalUnits = total;
    }
}
