using Cosmere;
using Cosmere.Core.Ability;
using Cosmere.Core.Gizmo;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Allomancy.Hediff;
using Cosmere.System.Scadrial.Gene;
using Verse;

namespace Cosmere.System.Scadrial.Gizmo;

[StaticConstructorOnStartup]
public class AllomanticAbilitySubGizmo
    : AbilitySubGizmo<Allomancer, AllomanticHediff> {
    public AllomanticAbilitySubGizmo() { }

    public AllomanticAbilitySubGizmo(
        Verse.Gizmo parent,
        Allomancer gene,
        AbstractAbility<Allomancer, AllomanticHediff> ability
    ) : base(parent, gene, ability) { }

    protected override TaggedString GetPowerUpDisplay() {
        TaggedString flareOrDeflare =
            (ability.status == BurningStatus.Flaring ? "CS_Deflare" : "CS_Flare").Translate();

        return "CS_PressToFlare"
            .Translate(flareOrDeflare.Named("FLARE"), gene.metal.label.Named("METAL"))
            .Colorize(ColoredText.GeneColor);
    }
}