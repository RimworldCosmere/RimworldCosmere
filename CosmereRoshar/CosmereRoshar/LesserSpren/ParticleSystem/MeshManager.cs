using System;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.LesserSpren.ParticleSystem;

public class MeshManager {
    private readonly float altitudeLayer;
    private readonly Func<Vector3, bool> isPositionValid;
    private readonly Map map;
    private readonly int totalColumns;

    public List<IntVec3> FinalValidCells = [];

    public MeshManager(Map map, Func<Vector3, bool> isPositionValid) {
        this.map = map;
        altitudeLayer = AltitudeLayer.VisEffects.AltitudeFor();
        this.isPositionValid = isPositionValid;
        FinalValidCells = [];
        totalColumns = this.map.Size.x;
    }

    public void Reset() {
        FinalValidCells.Clear();
    }

    public bool ValidateCells() {
        for (int currentColumn = 0; currentColumn < totalColumns; currentColumn++) {
            // Temporary list to hold valid cells for this column
            List<IntVec3> columnValidCells = [];
            for (int zCoord = 0; zCoord < map.Size.z; zCoord++) {
                Vector3 cellPos = new Vector3(currentColumn, 0, zCoord);
                Vector3 worldPos = cellPos + new Vector3(0, altitudeLayer, 0);
                if (isPositionValid(worldPos)) {
                    columnValidCells.Add(new IntVec3(currentColumn, 0, zCoord));
                }
            }

            FinalValidCells.AddRange(columnValidCells);
        }

        return true;
    }

    public void ConstructMesh(Mesh mesh) {
        List<Vector3> vertices = [];
        List<int> triangles = [];

        int vertexIndex = 0;
        foreach (IntVec3 cell in FinalValidCells) {
            Vector3 worldPosition = cell.ToVector3() + new Vector3(0, altitudeLayer, 0);

            vertices.Add(worldPosition + new Vector3(-0.5f, 0, -0.5f)); // Bottom-left
            vertices.Add(worldPosition + new Vector3(0.5f, 0, -0.5f)); // Bottom-right
            vertices.Add(worldPosition + new Vector3(0.5f, 0, 0.5f)); // Top-right
            vertices.Add(worldPosition + new Vector3(-0.5f, 0, 0.5f)); // Top-left

            triangles.Add(vertexIndex + 0);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);

            triangles.Add(vertexIndex + 0);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 3);

            vertexIndex += 4;
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }
}