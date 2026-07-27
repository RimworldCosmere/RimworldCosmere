namespace Cosmere.Core.Ability.Autocast;

public enum AutocastTriggerKind {
    HealthPercent,
    ReservePercent,
    Drafted,

    // Cells to the closest hostile pawn. Reads as "an enemy is within N".
    EnemyProximity,

    // Cells to the closest pawn of the same faction.
    AllyProximity,
}
