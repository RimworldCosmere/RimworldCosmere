namespace Cosmere.Core.Ability;

/// <summary>
///     A non-generic view of a sustained ability's on/off state, so UI in Core can turn one off
///     without naming the shard-specific gene and hediff types <see cref="AbstractAbility{TGene,THediff}" />
///     is closed over.
/// </summary>
public interface IToggleableAbility {
    bool IsToggleable { get; }

    bool IsActive { get; }

    void TurnOff();
}
