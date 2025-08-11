using UnityEngine;
using Verse;

namespace Cosmere.Roshar.Map;

public class CellValidator(Verse.Map map) {
    private readonly Verse.Map map = map;

    /**
     * Add logic for spren here
     */
    public bool IsCellValidForParticleEmissionMesh(IntVec3 position) {
        return Rand.Chance(1 / 3f);
    }

    public bool IsCellValidForParticleEmissionMesh(Vector3 position) {
        return IsCellValidForParticleEmissionMesh(position.ToIntVec3());
    }
}