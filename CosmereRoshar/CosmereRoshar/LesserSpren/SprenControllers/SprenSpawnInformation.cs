using Verse;

namespace Cosmere.Roshar.LesserSpren.SprenControllers;

public record SprenSpawnInformation(
    Map? map,
    IntVec3? position,
    float spawnChance,
    int minParticles,
    int maxParticles,
    float maxSpreadDistance = 0.5f,
    float movementSpeed = 5f,
    float randomDirectionAmount = 0.3f
) {
    public readonly Map? map = map;
    public readonly int maxParticles = maxParticles;
    public readonly float maxSpreadDistance = maxSpreadDistance;
    public readonly int minParticles = minParticles;
    public readonly float movementSpeed = movementSpeed;
    public readonly float randomDirectionAmount = randomDirectionAmount;
    public readonly float spawnChance = spawnChance;
    public IntVec3? position = position;

    public SprenSpawnInformation With(
        Map? map = null,
        IntVec3? position = null,
        float? spawnChance = null,
        int? minParticles = null,
        int? maxParticles = null,
        float? maxSpreadDistance = null,
        float? movementSpeed = null,
        float? randomDirectionAmount = null
    ) {
        return new SprenSpawnInformation(
            map ?? this.map,
            position ?? this.position,
            spawnChance ?? this.spawnChance,
            minParticles ?? this.minParticles,
            maxParticles ?? this.maxParticles,
            maxSpreadDistance ?? this.maxSpreadDistance,
            movementSpeed ?? this.movementSpeed,
            randomDirectionAmount ?? this.randomDirectionAmount
        );
    }
}